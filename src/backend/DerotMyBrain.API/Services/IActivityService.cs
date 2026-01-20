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
    Task<IEnumerable<UserActivity>> GetTrackedActivitiesAsync(string userId);
    Task DeleteActivityAsync(string userId, string activityId);
    
    // Tracking Operations
    Task TrackActivityAsync(string userId, string activityId);
    Task UntrackActivityAsync(string userId, string activityId);
    
    // Dashboard Operations
    Task<UserStatisticsDto> GetStatisticsAsync(string userId);
    Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365);
    Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10);
}
