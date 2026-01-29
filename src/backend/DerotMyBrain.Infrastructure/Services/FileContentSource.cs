using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;

namespace DerotMyBrain.Infrastructure.Services;

using DerotMyBrain.Core.Utils;

public class FileContentSource : IContentSource
{
    private readonly ILogger<FileContentSource> _logger;
    private readonly ITextExtractor _textExtractor;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentRepository _documentRepository;

    public FileContentSource(
        ILogger<FileContentSource> logger, 
        ITextExtractor textExtractor, 
        IFileStorageService fileStorageService,
        IDocumentRepository documentRepository)
    {
        _logger = logger;
        _textExtractor = textExtractor;
        _fileStorageService = fileStorageService;
        _documentRepository = documentRepository;
    }

    public bool CanHandle(SourceType sourceType)
    {
        return sourceType == SourceType.Document;
    }

    public async Task<ContentResult> GetContentAsync(Source source)
    {
        _logger.LogInformation("Reading file content for Source {SourceId}", source.Id);

        try 
        {
            var document = await _documentRepository.GetBySourceIdAsync(source.UserId, source.Id);
            if (document == null)
            {
                throw new FileNotFoundException($"No document found for Source ID {source.Id}");
            }

            // Get stream from storage service
            using var stream = await _fileStorageService.GetFileStreamAsync(document.StoragePath);
            _logger.LogInformation("File stream opened for {Path}. Length: {Length}", document.StoragePath, stream.Length);
            
            // Extract text
            var text = _textExtractor.ExtractText(stream, document.FileType);
            _logger.LogInformation("Extracted text length: {Length}", text?.Length ?? 0);
            
            return new ContentResult
            {
                Title = document.DisplayTitle,
                TextContent = text,
                SourceType = "Document",
                SourceUrl = source.ExternalId ?? document.StoragePath
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file for source {SourceId}", source.Id);
            
            return new ContentResult
            {
                Title = "Error Reading File",
                TextContent = $"Error reading file content: {ex.Message}",
                SourceType = "Error",
                SourceUrl = source.Id
            };
        }
    }
}
