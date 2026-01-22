using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IActivityService
{
    // New Two-Phase Workflow
    Task<ContentResult> StartReadingAsync(string userId, StartActivityRequest request);
    Task<QuizDto> GenerateQuizAsync(string userId, string activityId);
    
    // Existing Operations (Refactored)
    Task<UserStatisticsDto> GetStatisticsAsync(string userId);
    Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365);
    Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10);
    Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId);
    Task DeleteActivityAsync(string userId, string activityId);
    Task<IEnumerable<UserActivity>> GetAllActivitiesAsync(string userId);
}
