using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.API.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _repository;
    private readonly ITrackedTopicRepository _trackedTopicRepository;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        IActivityRepository repository, 
        ITrackedTopicRepository trackedTopicRepository,
        ILogger<ActivityService> logger)
    {
        _repository = repository;
        _trackedTopicRepository = trackedTopicRepository;
        _logger = logger;
    }

    public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
    {
        _logger.LogInformation("Creating {Type} activity for user {UserId}, topic {Topic}", 
            dto.Type, userId, dto.Topic);

        var activity = new UserActivity
        {
            UserId = userId,
            Topic = dto.Topic,
            WikipediaUrl = dto.WikipediaUrl,
            Type = dto.Type,
            SessionDate = DateTime.UtcNow,
            Score = dto.Score,
            TotalQuestions = dto.TotalQuestions,
            LlmModelName = dto.LlmModelName,
            LlmVersion = dto.LlmVersion
        };

        await _repository.CreateAsync(activity);

        // Update TrackedTopic if this topic is tracked
        if (await IsTopicTrackedAsync(userId, dto.Topic))
        {
            await UpdateTrackedTopicCacheAsync(userId, dto.Topic, activity);
        }

        _logger.LogInformation("Activity created: {ActivityId}", activity.Id);
        return activity;
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

        activity.Score = dto.Score;
        activity.TotalQuestions = dto.TotalQuestions;
        activity.LlmModelName = dto.LlmModelName ?? activity.LlmModelName;
        activity.LlmVersion = dto.LlmVersion ?? activity.LlmVersion;

        await _repository.UpdateAsync(activity);

        // Update TrackedTopic cache if needed
        if (await IsTopicTrackedAsync(userId, activity.Topic))
        {
            // Re-syncing cache for update is tricky if we don't know if this was the "Best"
            // For now, let's just trigger a potential update if it's a quiz
            if (activity.Type == "Quiz")
            {
                await UpdateTrackedTopicCacheAsync(userId, activity.Topic, activity);
            }
        }

        return activity;
    }

    public async Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId)
    {
        return await _repository.GetByIdAsync(userId, activityId);
    }

    public async Task<IEnumerable<UserActivity>> GetAllActivitiesAsync(string userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic)
    {
        return await _repository.GetAllForTopicAsync(userId, topic);
    }

    public async Task DeleteActivityAsync(string userId, string activityId)
    {
        _logger.LogInformation("Deleting activity {ActivityId} for user {UserId}", activityId, userId);
        await _repository.DeleteAsync(userId, activityId);
    }

    public async Task<bool> IsTopicTrackedAsync(string userId, string topic)
    {
        return await _trackedTopicRepository.ExistsAsync(userId, topic);
    }

    private async Task UpdateTrackedTopicCacheAsync(string userId, string topic, UserActivity newSession)
    {
        var tracked = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        if (tracked == null) return;
        
        if (newSession.Type == "Read")
        {
            tracked.TotalReadSessions++;
            tracked.LastReadDate = newSession.SessionDate;
            if (tracked.FirstReadDate == null)
                tracked.FirstReadDate = newSession.SessionDate;
        }
        else if (newSession.Type == "Quiz")
        {
            tracked.TotalQuizAttempts++;
            tracked.LastAttemptDate = newSession.SessionDate;
            if (tracked.FirstAttemptDate == null)
                tracked.FirstAttemptDate = newSession.SessionDate;
            
            // Update best score if this is a new record
            if (tracked.BestScore == null || (newSession.Score.HasValue && newSession.Score > tracked.BestScore))
            {
                tracked.BestScore = newSession.Score;
                tracked.TotalQuestions = newSession.TotalQuestions;
                tracked.BestScoreDate = newSession.SessionDate;
                
                _logger.LogInformation("🎉 New best score for topic {Topic}: {Score}/{Total}", 
                    topic, tracked.BestScore, tracked.TotalQuestions);
            }
        }
        
        await _trackedTopicRepository.UpdateAsync(tracked);
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
