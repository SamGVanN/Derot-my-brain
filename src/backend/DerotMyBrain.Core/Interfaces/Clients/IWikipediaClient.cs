using DerotMyBrain.Core.DTOs;

namespace DerotMyBrain.Core.Interfaces.Clients;

public interface IWikipediaClient
{
    Task<IEnumerable<WikipediaArticleDto>> GetRandomArticlesWithTeasersAsync(int count, string lang = "en");
    Task<ContentResult> GetArticleRichContentAsync(string title, string lang = "en");
}
