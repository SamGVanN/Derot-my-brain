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
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository repository,
        ITextExtractor textExtractor,
        IFileStorageService fileStorageService,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _textExtractor = textExtractor;
        _fileStorageService = fileStorageService;
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

        var sourceHash = SourceHasher.GenerateHash(SourceType.Document, storagePath);

        var document = new Document
        {
            UserId = userId,
            FileName = fileName,
            FileType = Path.GetExtension(fileName).ToLowerInvariant(),
            FileSize = fileSize,
            UploadDate = DateTime.UtcNow,
            StoragePath = storagePath, 
            DisplayTitle = Path.GetFileNameWithoutExtension(fileName),
            SourceHash = sourceHash
        };

        // 3. Persist to DB
        return await _repository.CreateAsync(document);
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
    }

    public async Task<string> GetDocumentContentAsync(string userId, string sourceHash)
    {
        var doc = await _repository.GetBySourceHashAsync(userId, sourceHash);
        if (doc == null) throw new FileNotFoundException("Document not found in database.");

        // Get stream from storage service
        using var stream = await _fileStorageService.GetFileStreamAsync(doc.StoragePath);
        
        // Use the extractor service
        return _textExtractor.ExtractText(stream, doc.FileType);
    }
}
