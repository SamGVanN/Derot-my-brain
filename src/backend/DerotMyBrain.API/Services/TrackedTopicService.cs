using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.API.Services;

/// <summary>
/// Service for managing tracked topics.
/// </summary>
public class TrackedTopicService : ITrackedTopicService
{
    private readonly ITrackedTopicRepository _trackedTopicRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<TrackedTopicService> _logger;
    
    public TrackedTopicService(
        ITrackedTopicRepository trackedTopicRepository,
        IActivityRepository activityRepository,
        ILogger<TrackedTopicService> logger)
    {
        _trackedTopicRepository = trackedTopicRepository;
        _activityRepository = activityRepository;
        _logger = logger;
    }
    
    public async Task<TrackedTopicDto> TrackTopicAsync(string userId, string topic, string wikipediaUrl)
    {
        _logger.LogInformation("Tracking topic for user {UserId}: {Topic}", userId, topic);
        
        // Check if already tracked (idempotent)
        var existing = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        if (existing != null)
        {
            _logger.LogWarning("Topic {Topic} is already tracked by user {UserId}", topic, userId);
            return MapToDto(existing);
        }
        
        // Create TrackedTopic entry
        var trackedTopic = new TrackedTopic
        {
            UserId = userId,
            Topic = topic,
            WikipediaUrl = wikipediaUrl,
            TrackedDate = DateTime.UtcNow
        };
        
        // Rebuild aggregated data from existing UserActivity history
        var allSessions = await _activityRepository.GetAllForTopicAsync(userId, topic);
        
        foreach (var session in allSessions)
        {
            if (session.Type == "Read")
            {
                trackedTopic.TotalReadSessions++;
                trackedTopic.LastReadDate = session.SessionDate;
                if (trackedTopic.FirstReadDate == null || session.SessionDate < trackedTopic.FirstReadDate)
                    trackedTopic.FirstReadDate = session.SessionDate;
            }
            else if (session.Type == "Quiz")
            {
                trackedTopic.TotalQuizAttempts++;
                trackedTopic.LastAttemptDate = session.SessionDate;
                if (trackedTopic.FirstAttemptDate == null || session.SessionDate < trackedTopic.FirstAttemptDate)
                    trackedTopic.FirstAttemptDate = session.SessionDate;
                
                if (trackedTopic.BestScore == null || (session.Score.HasValue && session.Score > trackedTopic.BestScore))
                {
                    trackedTopic.BestScore = session.Score;
                    trackedTopic.TotalQuestions = session.TotalQuestions;
                    trackedTopic.BestScoreDate = session.SessionDate;
                }
            }
        }
        
        await _trackedTopicRepository.CreateAsync(trackedTopic);
        
        _logger.LogInformation("Topic tracked: {TrackedTopicId}, rebuilt from {SessionCount} sessions", 
            trackedTopic.Id, allSessions.Count());
        
        return MapToDto(trackedTopic);
    }
    
    public async Task UntrackTopicAsync(string userId, string topic)
    {
        _logger.LogInformation("Untracking topic for user {UserId}: {Topic}", userId, topic);
        
        var tracked = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        if (tracked == null)
        {
            _logger.LogWarning("Topic {Topic} is not tracked by user {UserId}", topic, userId);
            return;
        }
        
        await _trackedTopicRepository.DeleteAsync(tracked.Id);
        
        _logger.LogInformation("Topic untracked: {Topic}", topic);
    }
    
    public async Task<IEnumerable<TrackedTopicDto>> GetAllTrackedTopicsAsync(string userId)
    {
        var trackedTopics = await _trackedTopicRepository.GetAllAsync(userId);
        return trackedTopics.Select(MapToDto);
    }
    
    public async Task<TrackedTopicDto?> GetTrackedTopicAsync(string userId, string topic)
    {
        var trackedTopic = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        return trackedTopic != null ? MapToDto(trackedTopic) : null;
    }
    
    private static TrackedTopicDto MapToDto(TrackedTopic entity)
    {
        return new TrackedTopicDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Topic = entity.Topic,
            WikipediaUrl = entity.WikipediaUrl,
            TrackedDate = entity.TrackedDate,
            TotalReadSessions = entity.TotalReadSessions,
            TotalQuizAttempts = entity.TotalQuizAttempts,
            FirstReadDate = entity.FirstReadDate,
            LastReadDate = entity.LastReadDate,
            FirstAttemptDate = entity.FirstAttemptDate,
            LastAttemptDate = entity.LastAttemptDate,
            BestScore = entity.BestScore,
            TotalQuestions = entity.TotalQuestions,
            BestScoreDate = entity.BestScoreDate
        };
    }
}
