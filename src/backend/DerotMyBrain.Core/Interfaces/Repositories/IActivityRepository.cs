using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user activity data access.
/// Provides CRUD operations and dashboard queries.
/// </summary>
public interface IActivityRepository
{
    // CRUD Operations
    
    /// <summary>
    /// Gets all activities for a user, ordered by session date descending.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>Collection of user activities.</returns>
    Task<IEnumerable<UserActivity>> GetAllAsync(string userId);
    
    /// <summary>
    /// Gets all activities for a specific content (by source ID) for evolution tracking.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="sourceId">Deterministic ID of the source.</param>
    /// <returns>Collection of activities for the content, ordered by session date ascending.</returns>
    Task<IEnumerable<UserActivity>> GetAllForContentAsync(string userId, string sourceId);
    
    /// <summary>
    /// Gets a specific activity by ID for a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="activityId">Activity identifier.</param>
    /// <returns>User activity if found, null otherwise.</returns>
    Task<UserActivity?> GetByIdAsync(string userId, string activityId);
    
    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="activity">Activity to create.</param>
    /// <returns>Created activity.</returns>
    Task<UserActivity> CreateAsync(UserActivity activity);
    
    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="activity">Activity to update.</param>
    /// <returns>Updated activity.</returns>
    Task<UserActivity> UpdateAsync(UserActivity activity);
    
    /// <summary>
    /// Deletes an activity.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="activityId">Activity identifier.</param>
    Task DeleteAsync(string userId, string activityId);

    // Source Operations
    Task<Source?> GetSourceByIdAsync(string sourceId);
    Task<Source> CreateSourceAsync(Source source);
    Task<Source> UpdateSourceAsync(Source source);
    Task<IEnumerable<Source>> GetTrackedSourcesAsync(string userId);
    Task<OnlineResource> CreateOnlineResourceAsync(OnlineResource resource);
    Task<OnlineResource?> GetOnlineResourceBySourceIdAsync(string sourceId);

    // Session Operations
    Task<UserSession?> GetSessionByIdAsync(string userId, string sessionId);
    Task<UserSession> CreateSessionAsync(UserSession session);
    Task<UserSession> UpdateSessionAsync(UserSession session);
    Task<UserSession?> GetLastActiveSessionAsync(string userId, string sourceId);
    
    // Dashboard Queries
    
    /// <summary>
    /// Gets aggregated statistics for a user's activities.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>User statistics including counts, last activity, and best score.</returns>
    Task<UserStatisticsDto> GetStatisticsAsync(string userId);
    
    /// <summary>
    /// Gets activity calendar data for the specified number of days.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="days">Number of days to include (from today backwards).</param>
    /// <returns>Collection of dates with activity counts.</returns>
    Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days);
    
    /// <summary>
    /// Gets the top N scores for a user, ordered by percentage descending.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="limit">Maximum number of scores to return.</param>
    /// <returns>Collection of top scores.</returns>
    Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit);
}
