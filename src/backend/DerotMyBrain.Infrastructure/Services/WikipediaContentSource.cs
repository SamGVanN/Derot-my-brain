using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Clients;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class WikipediaContentSource : IContentSource
{
    private readonly IWikipediaClient _wikipediaClient;
    private readonly ILogger<WikipediaContentSource> _logger;

    public WikipediaContentSource(IWikipediaClient wikipediaClient, ILogger<WikipediaContentSource> logger)
    {
        _wikipediaClient = wikipediaClient;
        _logger = logger;
    }

    public bool CanHandle(string sourceId)
    {
        return sourceId.Contains("wikipedia.org") || 
               sourceId.Equals("RandomWiki", StringComparison.OrdinalIgnoreCase) || 
               sourceId.Equals("Wikipedia", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ContentResult> GetContentAsync(string sourceId)
    {
        if (sourceId.Equals("RandomWiki", StringComparison.OrdinalIgnoreCase) || 
            sourceId.Equals("Wikipedia", StringComparison.OrdinalIgnoreCase))
        {
            var randoms = await _wikipediaClient.GetRandomArticlesWithTeasersAsync(1);
            var first = randoms.FirstOrDefault();
            if (first == null) throw new Exception("Failed to fetch random Wikipedia article");
            
            return await _wikipediaClient.GetArticleRichContentAsync(first.Title);
        }
        
        // title extraction is handled by WikipediaService, but for IContentSource we might be called with URL
        // ContentSource is a lower level interface used by ActivityService.
        // Let's keep a bit of logic here or rely on IWikipediaClient's rich content if it can handle URLs eventually.
        
        string title = sourceId;
        string lang = "en";

        if (sourceId.Contains("wikipedia.org"))
        {
            var uri = new Uri(sourceId);
            title = uri.Segments.Last();
            title = System.Net.WebUtility.UrlDecode(title);
            var hostParts = uri.Host.Split('.');
            if (hostParts.Length >= 3) lang = hostParts[0];
        }

        return await _wikipediaClient.GetArticleRichContentAsync(title, lang);
    }
}
