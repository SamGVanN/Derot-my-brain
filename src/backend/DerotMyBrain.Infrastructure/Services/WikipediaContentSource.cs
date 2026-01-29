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
        string title = source.ExternalId ?? "";

        // Determine language:
        // 1. From URL if present
        if (Uri.TryCreate(source.ExternalId, UriKind.Absolute, out var uri))
        {
            title = uri.Segments.Last();
            title = System.Net.WebUtility.UrlDecode(title);
            var hostParts = uri.Host.Split('.');
            if (hostParts.Length >= 3) lang = hostParts[0];
        }
        else
        {
             // 2. From User Preferences if available
             var user = await _userRepository.GetByIdAsync(source.UserId);
             if (user?.Preferences != null)
             {
                 lang = user.Preferences.Language;
             }
             
             if (!string.IsNullOrEmpty(source.ExternalId))
             {
                 title = source.ExternalId;
             }
        }

        return await _wikipediaClient.GetArticleRichContentAsync(title, lang);
    }
}
