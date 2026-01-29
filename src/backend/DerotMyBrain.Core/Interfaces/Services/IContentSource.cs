using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IContentSource
{
    Task<ContentResult> GetContentAsync(Source source);
    
    bool CanHandle(SourceType sourceType);
}
