using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IWikipediaService
{
    Task<UserActivity> ExploreAsync(string userId);
    Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceUrl, string? originExploreId, int? backlogAddsCount);
}
