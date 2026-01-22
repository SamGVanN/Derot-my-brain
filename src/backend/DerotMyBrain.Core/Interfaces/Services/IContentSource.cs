using DerotMyBrain.Core.DTOs;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IContentSource
{
    // sourceId can be a URL, a FilePath, or a serialized filter string
    Task<ContentResult> GetContentAsync(string sourceId);
    
    // Check if this source handles the given input (e.g., "http" vs "file://")
    bool CanHandle(string sourceId);
}
