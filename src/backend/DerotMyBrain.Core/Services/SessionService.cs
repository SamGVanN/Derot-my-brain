using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

public class SessionService : ISessionService
{
    private readonly IActivityRepository _repository;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IActivityRepository repository, ILogger<SessionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserSessionDto?> GetSessionByIdAsync(string userId, string sessionId)
    {
        var session = await _repository.GetSessionByIdAsync(userId, sessionId);
        return session != null ? MapToSessionDto(session) : null;
    }

    public async Task<IEnumerable<UserSessionDto>> GetSessionsByUserIdAsync(string userId)
    {
        var sessions = await _repository.GetSessionsByUserIdAsync(userId);
        return sessions.Select(MapToSessionDto);
    }

    public async Task StopSessionAsync(string userId, string sessionId)
    {
        var session = await _repository.GetSessionByIdAsync(userId, sessionId);
        if (session == null) throw new KeyNotFoundException($"Session {sessionId} not found");

        session.Status = SessionStatus.Stopped;
        session.EndedAt = DateTime.UtcNow;
        await _repository.UpdateSessionAsync(session);

        _logger.LogInformation("Session {SessionId} stopped for user {UserId}", sessionId, userId);
    }

    private UserSessionDto MapToSessionDto(UserSession s)
    {
        return new UserSessionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            TargetSourceId = s.TargetSourceId,
            TargetTopicId = s.TargetTopicId,
            StartedAt = s.StartedAt,
            EndedAt = s.EndedAt,
            Status = s.Status,
            TotalDurationSeconds = s.TotalDurationSeconds,
            Activities = s.Activities.Select(a => MapToActivityDto(a)).ToList()
        };
    }

    private UserActivityDto MapToActivityDto(UserActivity a)
    {
        var source = a.Source ?? a.UserSession?.TargetSource;
        bool isTracked = source?.IsTracked ?? false;

        return new UserActivityDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserSessionId = a.UserSessionId,
            Title = a.Title,
            Description = a.Description,
            SourceId = source?.Id ?? string.Empty,
            ExternalId = source?.ExternalId ?? string.Empty,
            SourceType = source?.Type ?? SourceType.Custom,
            DisplayTitle = source?.DisplayTitle ?? string.Empty,
            Url = source?.OnlineResource?.URL ?? (source?.Type == SourceType.Wikipedia ? $"https://en.wikipedia.org/wiki/{source.ExternalId}" : string.Empty),
            Type = a.Type,
            SessionDateStart = a.SessionDateStart,
            SessionDateEnd = a.SessionDateEnd,
            DurationSeconds = a.DurationSeconds,
            Score = a.Score,
            QuestionCount = a.QuestionCount,
            ScorePercentage = a.ScorePercentage,
            IsNewBestScore = a.IsNewBestScore,
            IsBaseline = a.IsBaseline,
            IsCurrentBest = false, 
            IsCompleted = a.IsCompleted,
            LlmModelName = a.LlmModelName,
            LlmVersion = a.LlmVersion,
            IsTracked = isTracked,
            ArticleContent = source?.TextContent,
            Payload = a.Payload,
            ResultingReadActivityId = a.ResultingReadActivityId,
            ResultingReadSourceName = a.ResultingReadActivity?.Title
        };
    }
}
