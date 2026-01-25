using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

using DerotMyBrain.Core.Utils;

public class FileContentSource : IContentSource
{
    private readonly ILogger<FileContentSource> _logger;
    private readonly ITextExtractor _textExtractor;

    public FileContentSource(ILogger<FileContentSource> logger, ITextExtractor textExtractor)
    {
        _logger = logger;
        _textExtractor = textExtractor;
    }

    public bool CanHandle(string sourceId)
    {
        return sourceId.StartsWith("file://") || 
               sourceId.EndsWith(".pdf") || 
               sourceId.EndsWith(".txt") ||
               sourceId.EndsWith(".docx") ||
               sourceId.EndsWith(".odt") ||
               sourceId.Equals("File", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ContentResult> GetContentAsync(string sourceId)
    {
        _logger.LogInformation("Reading file content from {SourceId}", sourceId);

        // Remove prefix if present to get path
        var filePath = sourceId.Replace("file://", "");
        
        try 
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var text = _textExtractor.ExtractText(filePath, extension);
            
            return await Task.FromResult(new ContentResult
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                TextContent = text,
                SourceType = "Document",
                SourceUrl = sourceId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file {FilePath}", filePath);
            // Fallback or rethrow?
            // For now, return error as content to inform agent/user
            return new ContentResult
            {
                Title = "Error Reading File",
                TextContent = $"Error reading file content: {ex.Message}",
                SourceType = "Error",
                SourceUrl = sourceId
            };
        }
    }
}
