using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IUserFocusService
{
    /// <summary>
    /// Tracks a topic (UserFocus) for a user.
    /// </summary>
    Task<UserFocus> TrackTopicAsync(string userId, string sourceId, SourceType sourceType, string displayTitle);
    
    /// <summary>
    /// Untracks a topic (UserFocus) for a user.
    /// </summary>
    Task UntrackTopicAsync(string userId, string sourceHash);
    
    /// <summary>
    /// Gets all focuses for a user.
    /// </summary>
    Task<IEnumerable<UserFocus>> GetAllFocusesAsync(string userId);
    
    /// <summary>
    /// Gets a specific focus for a user by its content hash.
    /// </summary>
    Task<UserFocus?> GetFocusAsync(string userId, string sourceHash);
    
    /// <summary>
    /// Rebuilds the aggregated statistics for a focus from history.
    /// </summary>
    Task RebuildStatsAsync(string userId, string sourceHash);
    
    /// <summary>
    /// Updates the aggregated statistics of a focus after a new activity.
    /// </summary>
    Task UpdateStatsAsync(string userId, string sourceHash, UserActivity activity);
}
