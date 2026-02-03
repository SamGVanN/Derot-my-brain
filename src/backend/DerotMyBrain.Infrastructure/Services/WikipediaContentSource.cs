using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Clients;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class WikipediaContentSource : IContentSource
{
    private readonly ILogger<WikipediaContentSource> _logger;
    private readonly IWikipediaClient _wikipediaClient;
    private readonly IUserRepository _userRepository;

    public WikipediaContentSource(
        ILogger<WikipediaContentSource> logger, 
        IWikipediaClient wikipediaClient,
        IUserRepository userRepository)
    {
        _logger = logger;
        _wikipediaClient = wikipediaClient;
        _userRepository = userRepository;
    }

    public bool CanHandle(SourceType sourceType)
    {
        return sourceType == SourceType.Wikipedia;
    }

    public async Task<ContentResult> GetContentAsync(Source source)
    {
        string lang = "en";
        string title = string.Empty;

        // 1. Try to get title/lang from OnlineResource URL (The most reliable source)
        string? urlToParse = source.OnlineResource?.URL;
        
        // 2. Fallback: Check if ExternalId itself is a URL (legacy or document path)
        if (string.IsNullOrEmpty(urlToParse) && Uri.TryCreate(source.ExternalId, UriKind.Absolute, out _))
        {
            urlToParse = source.ExternalId;
        }

        if (!string.IsNullOrEmpty(urlToParse) && Uri.TryCreate(urlToParse, UriKind.Absolute, out var uri))
        {
            title = uri.Segments.Last();
            title = System.Net.WebUtility.UrlDecode(title);
            var hostParts = uri.Host.Split('.');
            if (hostParts.Length >= 3) lang = hostParts[0];
        }
        else
        {
            // 3. Last Resort: Use DisplayTitle or ExternalId as literal title if it's not a URL
            // This handles cases where ExternalId might be a Title in legacy data.
            // If ExternalId is a Hash, we are in trouble here, but OnlineResource should exist for Wikipedia.
            title = string.IsNullOrEmpty(source.DisplayTitle) ? source.ExternalId : source.DisplayTitle;
            
            var user = await _userRepository.GetByIdAsync(source.UserId);
            if (user?.Preferences != null)
            {
                lang = user.Preferences.Language;
            }
        }

        if (string.IsNullOrEmpty(title) || title.Length == 64 && !title.Contains("-")) // Looks like a hash
        {
             _logger.LogWarning("WikipediaContentSource: Resolved title '{Title}' looks like a hash or is empty for Source {SourceId}. Content may fail to load.", title, source.Id);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
             throw new ArgumentException($"WikipediaContentSource: Could not resolve a valid title for Source {source.Id}. ExternalId: {source.ExternalId}");
        }

        return await _wikipediaClient.GetArticleRichContentAsync(title, lang);
    }
}
