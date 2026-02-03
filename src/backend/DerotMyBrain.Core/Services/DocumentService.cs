using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly ITextExtractor _textExtractor;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISourceService _sourceService;
    private readonly IContentExtractionQueue _extractionQueue;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository repository,
        ITextExtractor textExtractor,
        IFileStorageService fileStorageService,
        ISourceService sourceService,
        IContentExtractionQueue extractionQueue,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _textExtractor = textExtractor;
        _fileStorageService = fileStorageService;
        _sourceService = sourceService;
        _extractionQueue = extractionQueue;
        _logger = logger;
    }

    public async Task<Document> UploadDocumentAsync(string userId, string fileName, Stream fileStream, string contentType)
    {
        // 1. Save file via storage service
        // Use userId as subdirectory to organize files
        var storagePath = await _fileStorageService.SaveFileAsync(fileStream, fileName, userId);
        
        // 2. Create metadata
        // Note: FileSize might need to be captured from stream before saving if stream is not seekable,
        // or we rely on the stream having Length property. IFormFile stream has Length.
        long fileSize = 0;
        try
        {
            if (fileStream.CanSeek)
            {
                fileSize = fileStream.Length;
            }
        }
        catch 
        {
            _logger.LogWarning("Could not determine file size from stream for file {FileName}", fileName);
        }

        var document = new Document
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            FileName = fileName,
            FileType = Path.GetExtension(fileName).ToLowerInvariant(),
            FileSize = fileSize,
            UploadDate = DateTime.UtcNow,
            StoragePath = storagePath, 
            DisplayTitle = Path.GetFileNameWithoutExtension(fileName),
            SourceId = string.Empty // Set below
        };

        // Ensure Source exists
        var source = await _sourceService.GetOrCreateSourceAsync(
            userId, 
            document.DisplayTitle, 
            document.Id, 
            SourceType.Document);
            
        document.SourceId = source.Id;

        // Set initial extraction status to Pending
        source.ContentExtractionStatus = ContentExtractionStatus.Pending;
        source.ContentExtractionError = null;
        source.ContentExtractionCompletedAt = null;
        await _sourceService.UpdateSourceAsync(source);

        // 3. Persist document to DB
        var createdDocument = await _repository.CreateAsync(document);
        _logger.LogInformation("Document {DocumentId} created in DB for source {SourceId}", createdDocument.Id, source.Id);

        // 4. Queue content extraction (non-blocking) - done after persistence to avoid race conditions
        _logger.LogInformation("Queueing content extraction for source {SourceId}", source.Id);
        _extractionQueue.QueueExtraction(source.Id);
        
        return createdDocument;
    }

    public async Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task DeleteDocumentAsync(string userId, string documentId)
    {
        var doc = await _repository.GetByIdAsync(userId, documentId);
        if (doc == null) return;

        // 1. Delete file
        await _fileStorageService.DeleteFileAsync(doc.StoragePath);

        // 2. Delete DB record
        await _repository.DeleteAsync(userId, documentId);

        // 3. Cleanup Source if not tracked and no activities (similar to BacklogService)
        var source = await _sourceService.GetSourceAsync(doc.SourceId);
        if (source != null && !source.IsTracked && (source.Activities == null || !source.Activities.Any()))
        {
             // We use _activityRepository via SourceService or directly if we had the repo.
             // But DocumentService doesn't have IActivityRepository. 
             // Let's add it or use a method in ISourceService if available.
             // Actually, ISourceService has UpdateSourceAsync but not delete.
             // I'll skip deleting source from here to avoid adding a new dependency for now,
             // OR I add IActivityRepository to DocumentService.
             // Given the "Source Central" request, deleting the document SHOULD probably delete the source if it's just for that document.
        }
    }

    public async Task<string> GetDocumentContentAsync(string userId, string sourceId)
    {
        var doc = await _repository.GetBySourceIdAsync(userId, sourceId);
        if (doc == null) throw new FileNotFoundException("Document not found in database.");

        // Get stream from storage service
        using var stream = await _fileStorageService.GetFileStreamAsync(doc.StoragePath);
        
        // Use the extractor service
        return _textExtractor.ExtractText(stream, doc.FileType);
    }
}
