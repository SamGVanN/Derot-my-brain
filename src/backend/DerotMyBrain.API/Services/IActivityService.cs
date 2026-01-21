using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Services;

public interface IActivityService
{
    // CRUD Operations
    Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto);
    Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto);
    Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId);
    Task<IEnumerable<UserActivity>> GetAllActivitiesAsync(string userId);
    Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic);
    Task DeleteActivityAsync(string userId, string activityId);
    
    // Tracking Operations
    /// <summary>
    /// Checks if a topic is tracked by the user.
    /// </summary>
    Task<bool> IsTopicTrackedAsync(string userId, string topic);
    
    // Dashboard Operations
    Task<UserStatisticsDto> GetStatisticsAsync(string userId);
    Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365);
    Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10);
}
