using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.API.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _repository;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(IActivityRepository repository, ILogger<ActivityService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
    {
        _logger.LogInformation("Creating activity for user {UserId} on topic {Topic}", userId, dto.Topic);

        var activity = new UserActivity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Topic = dto.Topic,
            WikipediaUrl = dto.WikipediaUrl,
            FirstAttemptDate = DateTime.UtcNow,
            LastAttemptDate = DateTime.UtcNow,
            LastScore = dto.LastScore,
            BestScore = dto.LastScore, // Initial best score is the first score
            TotalQuestions = dto.TotalQuestions,
            LlmModelName = dto.LlmModelName,
            LlmVersion = dto.LlmVersion,
            IsTracked = false,
            Type = dto.Type
        };

        return await _repository.CreateAsync(activity);
    }

    public async Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto)
    {
        _logger.LogInformation("Updating activity {ActivityId} for user {UserId}", activityId, userId);

        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null)
        {
            _logger.LogWarning("Activity {ActivityId} not found", activityId);
            throw new KeyNotFoundException($"Activity {activityId} not found");
        }

        activity.LastScore = dto.LastScore;
        activity.TotalQuestions = dto.TotalQuestions;
        activity.LastAttemptDate = DateTime.UtcNow;
        activity.LlmModelName = dto.LlmModelName ?? activity.LlmModelName;
        activity.LlmVersion = dto.LlmVersion ?? activity.LlmVersion;

        // Logic: BestScore is the max of current BestScore and new LastScore
        activity.BestScore = Math.Max(activity.BestScore, dto.LastScore);

        return await _repository.UpdateAsync(activity);
    }

    public async Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId)
    {
        return await _repository.GetByIdAsync(userId, activityId);
    }

    public async Task<IEnumerable<UserActivity>> GetAllActivitiesAsync(string userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<IEnumerable<UserActivity>> GetTrackedActivitiesAsync(string userId)
    {
        return await _repository.GetTrackedAsync(userId);
    }

    public async Task DeleteActivityAsync(string userId, string activityId)
    {
        _logger.LogInformation("Deleting activity {ActivityId} for user {UserId}", activityId, userId);
        await _repository.DeleteAsync(userId, activityId);
    }

    public async Task TrackActivityAsync(string userId, string activityId)
    {
        _logger.LogInformation("Tracking activity {ActivityId} for user {UserId}", activityId, userId);
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity != null)
        {
            activity.IsTracked = true;
            await _repository.UpdateAsync(activity);
        }
    }

    public async Task UntrackActivityAsync(string userId, string activityId)
    {
        _logger.LogInformation("Untracking activity {ActivityId} for user {UserId}", activityId, userId);
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity != null)
        {
            activity.IsTracked = false;
            await _repository.UpdateAsync(activity);
        }
    }

    public async Task<UserStatisticsDto> GetStatisticsAsync(string userId)
    {
        return await _repository.GetStatisticsAsync(userId);
    }

    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365)
    {
        return await _repository.GetActivityCalendarAsync(userId, days);
    }

    public async Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10)
    {
        return await _repository.GetTopScoresAsync(userId, limit);
    }
}
