using DerotMyBrain.API.DTOs;

namespace DerotMyBrain.API.Services;

/// <summary>
/// Service interface for tracked topic operations.
/// </summary>
public interface ITrackedTopicService
{
    /// <summary>
    /// Tracks a topic for the user. Rebuilds history from existing activities.
    /// </summary>
    Task<TrackedTopicDto> TrackTopicAsync(string userId, string topic, string wikipediaUrl);
    
    /// <summary>
    /// Untracks a topic. Preserves activity history.
    /// </summary>
    Task UntrackTopicAsync(string userId, string topic);
    
    /// <summary>
    /// Gets all tracked topics for a user.
    /// </summary>
    Task<IEnumerable<TrackedTopicDto>> GetAllTrackedTopicsAsync(string userId);
    
    /// <summary>
    /// Gets a specific tracked topic.
    /// </summary>
    Task<TrackedTopicDto?> GetTrackedTopicAsync(string userId, string topic);
}
