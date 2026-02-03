using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IActivityService
{
    // New Two-Phase Workflow
    Task<IEnumerable<WikipediaArticleDto>> GetExploreArticlesAsync(string userId, int count = 6);
    Task<UserActivity> ExploreAsync(string userId, string? title = null, string? sourceId = null, SourceType sourceType = SourceType.Custom, string? sessionId = null);
    Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceId, SourceType? sourceType, ActivityType activityType = ActivityType.Read, string? originExploreId = null, string? sessionId = null, int? backlogAddsCount = null, int? refreshCount = null, int? exploreDurationSeconds = null);
    Task<QuizDto> GenerateQuizAsync(string userId, string activityId);
    Task<QuizResultDto> SubmitQuizAsync(string userId, string activityId, QuizSubmissionDto submission);
    Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto);
    
    // Existing Operations (Refactored)
    Task<UserStatisticsDto> GetStatisticsAsync(string userId);
    Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365);
    Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10);
    Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId);
    Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto);
    Task DeleteActivityAsync(string userId, string activityId);
    Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(string userId);
    Task<IEnumerable<UserActivityDto>> GetAllForContentAsync(string userId, string sourceId);
    Task StopSessionAsync(string userId, string sessionId);
}
