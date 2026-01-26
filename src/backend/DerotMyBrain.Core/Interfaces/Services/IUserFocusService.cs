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
    Task UntrackTopicAsync(string userId, string sourceId);
    
    /// <summary>
    /// Gets all focuses for a user.
    /// </summary>
    Task<IEnumerable<UserFocus>> GetAllFocusesAsync(string userId);
    
    /// <summary>
    /// Gets a specific focus for a user by its source ID.
    /// </summary>
    Task<UserFocus?> GetFocusAsync(string userId, string sourceId);
    
    /// <summary>
    /// Rebuilds the aggregated statistics for a focus from history.
    /// </summary>
    Task RebuildStatsAsync(string userId, string sourceId);
    
    /// <summary>
    /// Updates the aggregated statistics of a focus after a new activity.
    /// </summary>
    Task UpdateStatsAsync(string userId, string sourceId, UserActivity activity);

    /// <summary>
    /// Toggles the pinned status of a focus área.
    /// </summary>
    Task<UserFocus?> TogglePinAsync(string userId, string sourceId);

    /// <summary>
    /// Toggles the archived status of a focus área.
    /// </summary>
    Task<UserFocus?> ToggleArchiveAsync(string userId, string sourceId);
}
