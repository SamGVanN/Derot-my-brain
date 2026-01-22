using System.Text.Json;
using System.Text.Json.Nodes;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class WikipediaContentSource : IContentSource
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WikipediaContentSource> _logger;
    private const string ApiBaseUrl = "https://en.wikipedia.org/w/api.php"; // Default to EN, logic could support multi-lang

    public WikipediaContentSource(HttpClient httpClient, ILogger<WikipediaContentSource> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public bool CanHandle(string sourceId)
    {
        return sourceId.Contains("wikipedia.org") || sourceId.Equals("RandomWiki", StringComparison.OrdinalIgnoreCase) || sourceId.Equals("Wikipedia", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ContentResult> GetContentAsync(string sourceId)
    {
        if (sourceId.Equals("RandomWiki", StringComparison.OrdinalIgnoreCase) || sourceId.Equals("Wikipedia", StringComparison.OrdinalIgnoreCase) || sourceId.StartsWith("{"))
        {
            return await GetRandomArticleAsync(sourceId);
        }
        else
        {
            return await GetArticleFromUrlAsync(sourceId);
        }
    }

    private async Task<ContentResult> GetRandomArticleAsync(string filterJson)
    {
        // Simple random fetch for now. Filters could be used to select category.
        // Wiki API: action=query&format=json&list=random&rnnamespace=0&rnlimit=1
        var url = $"{ApiBaseUrl}?action=query&format=json&list=random&rnnamespace=0&rnlimit=1";
        
        // TODO: Apply filters if filterJson has categories (using action=query&generator=random&grnnamespace=0&... is harder, maybe categorymembers)
        
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonNode.Parse(response);
        var randomPage = json?["query"]?["random"]?[0];
        
        if (randomPage == null) throw new Exception("Failed to fetch random Wikipedia article");
        
        var title = randomPage["title"]?.ToString() ?? "";
        return await GetArticleContentByTitleAsync(title);
    }

    private async Task<ContentResult> GetArticleFromUrlAsync(string url)
    {
        // Extract title from URL
        // https://en.wikipedia.org/wiki/Theory_of_relativity
        var uri = new Uri(url);
        var title = uri.Segments.Last(); 
        // Decode URL encoding
        title = System.Net.WebUtility.UrlDecode(title);
        
        return await GetArticleContentByTitleAsync(title);
    }

    private async Task<ContentResult> GetArticleContentByTitleAsync(string title)
    {
        // action=query&format=json&prop=extracts&explaintext=1&titles=...
        var url = $"{ApiBaseUrl}?action=query&format=json&prop=extracts&explaintext=1&titles={Uri.EscapeDataString(title)}";
        
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonNode.Parse(response);
        var pages = json?["query"]?["pages"]?.AsObject();
        var firstPage = pages?.FirstOrDefault().Value;
        
        if (firstPage == null) throw new Exception($"Failed to fetch content for {title}");
        
        var extract = firstPage["extract"]?.ToString() ?? "";
        var pageId = firstPage["pageid"]?.ToString();
        var realTitle = firstPage["title"]?.ToString() ?? title;
        
        return new ContentResult
        {
            Title = realTitle,
            TextContent = extract,
            SourceUrl = $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(realTitle)}",
            SourceType = "Wikipedia"
        };
    }
}
