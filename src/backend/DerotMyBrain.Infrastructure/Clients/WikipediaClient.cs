using System.Text.Json.Nodes;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DerotMyBrain.Infrastructure.Clients;

public class WikipediaClient : IWikipediaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WikipediaClient> _logger;
    private const string ApiBaseUrlTemplate = "https://{0}.wikipedia.org/w/api.php";


    public WikipediaClient(HttpClient httpClient, ILogger<WikipediaClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var email = configuration["Wikipedia:ContactEmail"];
        // var email = "SamGVanN@proton.me";
        var userAgent = "DerotMyBrain/1.0 (https://github.com/SamGVanN/Derot-my-brain)";
        
        if (!string.IsNullOrEmpty(email) && email != "your-email-here@proton.me")
        {
            userAgent = $"DerotMyBrain/1.0 (https://github.com/SamGVanN/Derot-my-brain; {email})";
        }

        // Wikipedia API requires a User-Agent header
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }
    }

    private string GetApiUrl(string lang) => string.Format(ApiBaseUrlTemplate, lang);

    public async Task<IEnumerable<WikipediaArticleDto>> GetRandomArticlesWithTeasersAsync(int count, string lang = "en")
    {
        // action=query&format=json&generator=random&grnnamespace=0&grnlimit=count&prop=extracts|pageimages&exintro=1&explaintext=1&exsentences=3&pithumbsize=300
        var url = $"{GetApiUrl(lang)}?action=query&format=json&generator=random&grnnamespace=0&grnlimit={count}&prop=extracts|pageimages&exintro=1&explaintext=1&exsentences=3&pithumbsize=300";

        _logger.LogInformation("Fetching random Wikipedia articles from: {Url}", url);
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            _logger.LogDebug("Wikipedia API response received: {ResponseLength} chars", response.Length);
            
            var json = JsonNode.Parse(response);
            var pages = json?["query"]?["pages"]?.AsObject();

            if (pages == null) return Enumerable.Empty<WikipediaArticleDto>();

            var results = new List<WikipediaArticleDto>();
            foreach (var page in pages)
            {
                var pageData = page.Value;
                if (pageData == null) continue;

                results.Add(new WikipediaArticleDto
                {
                    Title = pageData["title"]?.ToString() ?? "",
                    Summary = pageData["extract"]?.ToString(),
                    Lang = lang,
                    SourceUrl = $"https://{lang}.wikipedia.org/wiki/{Uri.EscapeDataString(pageData["title"]?.ToString() ?? "")}",
                    ImageUrl = pageData["thumbnail"]?["source"]?.ToString()
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching random articles from Wikipedia (lang: {Lang})", lang);
            return Enumerable.Empty<WikipediaArticleDto>();
        }
    }

    public async Task<ContentResult> GetArticleRichContentAsync(string title, string lang = "en")
    {
        // action=query&format=json&prop=extracts&explaintext=1&titles=title
        var url = $"{GetApiUrl(lang)}?action=query&format=json&prop=extracts&explaintext=1&titles={Uri.EscapeDataString(title)}";

        var response = await _httpClient.GetStringAsync(url);
        var json = JsonNode.Parse(response);
        var pages = json?["query"]?["pages"]?.AsObject();
        var firstPage = pages?.FirstOrDefault().Value;

        if (firstPage == null || firstPage["missing"] != null)
        {
            throw new KeyNotFoundException($"Wikipedia article '{title}' not found in language '{lang}'.");
        }

        var extract = firstPage["extract"]?.ToString() ?? "";
        var realTitle = firstPage["title"]?.ToString() ?? title;

        return new ContentResult
        {
            Title = realTitle,
            TextContent = extract,
            SourceUrl = $"https://{lang}.wikipedia.org/wiki/{Uri.EscapeDataString(realTitle)}",
            SourceType = "Wikipedia"
        };
    }
}
