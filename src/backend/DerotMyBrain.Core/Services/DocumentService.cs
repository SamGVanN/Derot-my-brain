using System.Text;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly ITextExtractor _textExtractor;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _storageRoot;

    public DocumentService(
        IDocumentRepository repository,
        ITextExtractor textExtractor,
        IConfiguration configuration,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _textExtractor = textExtractor;
        _logger = logger;
        
        // Determine storage path (similar to SQLite path logic)
        // Default to "Data/Documents" relative to execution
        var dataDirectory = configuration["DataDirectory"] ?? "Data";
        _storageRoot = Path.Combine(dataDirectory, "Documents");
        
        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<Document> UploadDocumentAsync(string userId, string fileName, Stream fileStream, string contentType)
    {
        // 1. Save file to disk
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var userDir = Path.Combine(_storageRoot, userId);
        if (!Directory.Exists(userDir))
        {
            Directory.CreateDirectory(userDir);
        }
        
        var filePath = Path.Combine(userDir, uniqueFileName);
        
        // Copy stream to file
        using (var destStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destStream);
        }
        
        // 2. Create metadata
        var fileInfo = new FileInfo(filePath);
        var relativePath = Path.Combine(userId, uniqueFileName);
        var sourceHash = SourceHasher.GenerateHash(SourceType.Document, relativePath);

        var document = new Document
        {
            UserId = userId,
            FileName = fileName,
            FileType = Path.GetExtension(fileName).ToLowerInvariant(),
            FileSize = fileInfo.Length,
            UploadDate = DateTime.UtcNow,
            StoragePath = relativePath, 
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
        var fullPath = Path.Combine(_storageRoot, doc.StoragePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        // 2. Delete DB record
        await _repository.DeleteAsync(userId, documentId);
    }

    public async Task<string> GetDocumentContentAsync(string userId, string sourceHash)
    {
        var doc = await _repository.GetBySourceHashAsync(userId, sourceHash);
        if (doc == null) throw new FileNotFoundException("Document not found in database.");

        var fullPath = Path.Combine(_storageRoot, doc.StoragePath);
        
        // Use the extractor service
        return _textExtractor.ExtractText(fullPath, doc.FileType);
    }
}
