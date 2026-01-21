using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Repositories;

/// <summary>
/// Repository interface for TrackedTopic operations.
/// </summary>
public interface ITrackedTopicRepository
{
    /// <summary>
    /// Gets a tracked topic by its unique identifier.
    /// </summary>
    Task<TrackedTopic?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a tracked topic by user ID and topic name.
    /// </summary>
    Task<TrackedTopic?> GetByTopicAsync(string userId, string topic);
    
    /// <summary>
    /// Gets all tracked topics for a user.
    /// </summary>
    Task<IEnumerable<TrackedTopic>> GetAllAsync(string userId);
    
    /// <summary>
    /// Creates a new tracked topic.
    /// </summary>
    Task<TrackedTopic> CreateAsync(TrackedTopic trackedTopic);
    
    /// <summary>
    /// Updates an existing tracked topic.
    /// </summary>
    Task UpdateAsync(TrackedTopic trackedTopic);
    
    /// <summary>
    /// Deletes a tracked topic by ID.
    /// </summary>
    Task DeleteAsync(string id);
    
    /// <summary>
    /// Checks if a topic is tracked by the user.
    /// </summary>
    Task<bool> ExistsAsync(string userId, string topic);
}
