using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;

namespace DerotMyBrain.Core.Services;

public class WikipediaService : IWikipediaService
{
    private readonly IActivityService _activityService;

    public WikipediaService(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public async Task<UserActivity> ExploreAsync(string userId)
    {
        var dto = new CreateActivityDto
        {
            Title = "DerotZone Explore",
            Description = "Exploration session",
            SourceId = DateTime.UtcNow.ToString("o"),
            SourceType = SourceType.Custom,
            Type = ActivityType.Explore,
            SessionDateStart = DateTime.UtcNow
        };

        return await _activityService.CreateActivityAsync(userId, dto);
    }

    public async Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceUrl, string? originExploreId, int? backlogAddsCount)
    {
        var dto = new CreateActivityDto
        {
            Title = title ?? sourceUrl ?? "Read",
            Description = "Read from Derot Zone",
            SourceId = sourceUrl ?? title ?? string.Empty,
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow,
            OriginExploreId = originExploreId,
            BacklogAddsCount = backlogAddsCount
        };

        return await _activityService.CreateActivityAsync(userId, dto);
    }
}
