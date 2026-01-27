using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IWikipediaService
{
    Task<IEnumerable<WikipediaArticleDto>> GetDiscoveryArticlesAsync(int count = 6, string lang = "en");
    Task<ContentResult> GetFullArticleAsync(string titleOrUrl);
}
