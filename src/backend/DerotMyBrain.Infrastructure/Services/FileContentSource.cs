using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class FileContentSource : IContentSource
{
    private readonly ILogger<FileContentSource> _logger;

    public FileContentSource(ILogger<FileContentSource> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(string sourceId)
    {
        // Simple check: starts with file path indicator or extension check
        return sourceId.StartsWith("file://") || sourceId.EndsWith(".txt") || sourceId.EndsWith(".pdf") || sourceId.Equals("File", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ContentResult> GetContentAsync(string sourceId)
    {
        // For now, return dummy content or implement reading local file if path provided
        // In real app, this might handle uploaded stream which doesn't fit simple string sourceId easily
        // unless sourceId is a TempFilePath.
        
        _logger.LogInformation("Reading file content from {SourceId}", sourceId);
        
        // Mock implementation
        return await Task.FromResult(new ContentResult
        {
            Title = "Uploaded Document",
            TextContent = "This is the content of the uploaded document. It contains information about Clean Architecture.",
            SourceType = "File",
            SourceUrl = sourceId
        });
    }
}
