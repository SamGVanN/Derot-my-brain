using Microsoft.Extensions.Logging;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Clients;
using DerotMyBrain.Core.Interfaces.Services;

namespace DerotMyBrain.Core.Services;

public class WikipediaService : IWikipediaService
{
    private readonly IWikipediaClient _wikipediaClient;
    private readonly ILogger<WikipediaService> _logger;

    public WikipediaService(IWikipediaClient wikipediaClient, ILogger<WikipediaService> logger)
    {
        _wikipediaClient = wikipediaClient;
        _logger = logger;
    }

    public async Task<IEnumerable<WikipediaArticleDto>> GetDiscoveryArticlesAsync(int count = 6, string lang = "en")
    {
        _logger.LogInformation("Getting {Count} discovery articles for language {Lang}", count, lang);
        var articles = await _wikipediaClient.GetRandomArticlesWithTeasersAsync(count, lang);
        var articleList = articles.ToList();
        _logger.LogInformation("Found {Count} discovery articles", articleList.Count);
        return articleList;
    }

    public async Task<ContentResult> GetFullArticleAsync(string titleOrUrl)
    {
        string title = titleOrUrl;
        string lang = "en"; //Needs fix : should be user profile language, and "en" only as fallback

        if (titleOrUrl.Contains("wikipedia.org"))
        {
            var uri = new Uri(titleOrUrl);
            title = uri.Segments.Last();
            title = System.Net.WebUtility.UrlDecode(title);
            
            // Try to extract language from subdomain (e.g., fr.wikipedia.org)
            var hostParts = uri.Host.Split('.');
            if (hostParts.Length >= 3)
            {
                lang = hostParts[0];
            }
        }

        return await _wikipediaClient.GetArticleRichContentAsync(title, lang);
    }
}
