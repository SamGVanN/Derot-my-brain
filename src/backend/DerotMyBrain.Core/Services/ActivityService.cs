using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Utils;

namespace DerotMyBrain.Core.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _repository;
    private readonly IEnumerable<IContentSource> _contentSources;
    private readonly ILlmService _llmService;
    private readonly IJsonSerializer _jsonSerializer;

    public ActivityService(
        IActivityRepository repository,
        IEnumerable<IContentSource> contentSources,
        ILlmService llmService,
        IJsonSerializer jsonSerializer)
    {
        _repository = repository;
        _contentSources = contentSources;
        _llmService = llmService;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<UserActivity> ExploreAsync(string userId, string? title = null, string? sourceId = null, SourceType sourceType = SourceType.Custom)
    {
        var dto = new CreateActivityDto
        {
            Title = title ?? "Exploration",
            Description = "Exploration session",
            SourceId = sourceId, 
            SourceType = sourceType,
            Type = ActivityType.Explore,
            SessionDateStart = DateTime.UtcNow
        };

        return await CreateActivityAsync(userId, dto);
    }

    public async Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceId, SourceType sourceType, string? originExploreId = null, int? backlogAddsCount = null, int? exploreDurationSeconds = null)
    {
        var sourceName = sourceType == SourceType.Wikipedia ? "Wikipedia" : "File"; 
        var source = _contentSources.FirstOrDefault(s => s.CanHandle(sourceName));
        if (source == null) throw new InvalidOperationException($"Content source for {sourceType} not found.");

        var content = await source.GetContentAsync(sourceId ?? title);

        var dto = new CreateActivityDto
        {
            Title = content.Title,
            Description = content.SourceType == "Wikipedia" ? "Read from Wikipedia" : "Read from Document",
            SourceId = content.SourceUrl,
            SourceType = sourceType,
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow,
            OriginExploreId = originExploreId,
            BacklogAddsCount = backlogAddsCount,
            ExploreDurationSeconds = exploreDurationSeconds
        };

        var activity = await CreateActivityAsync(userId, dto);
        
        activity.ArticleContent = content.TextContent;
        await _repository.UpdateAsync(activity);

        return activity;
    }

    public async Task<QuizDto> GenerateQuizAsync(string userId, string activityId)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException("Activity not found");

        string textToProcess = activity.ArticleContent ?? string.Empty;
        if (string.IsNullOrEmpty(textToProcess)) throw new InvalidOperationException("No content available to generate quiz.");

        var questionsJson = await _llmService.GenerateQuestionsAsync(textToProcess);
        var questions = _jsonSerializer.Deserialize<List<QuestionDto>>(questionsJson) ?? new List<QuestionDto>();

        return new QuizDto { Questions = questions };
    }

    public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
    {
        string? technicalSourceId = null;
        if (!string.IsNullOrEmpty(dto.SourceId))
        {
            technicalSourceId = SourceHasher.GenerateId(dto.SourceType, dto.SourceId);
            var source = await _repository.GetSourceByIdAsync(technicalSourceId);
            if (source == null)
            {
                source = new Source
                {
                    Id = technicalSourceId,
                    UserId = userId,
                    Type = dto.SourceType,
                    ExternalId = dto.SourceId,
                    DisplayTitle = dto.Title,
                    Url = dto.SourceType == SourceType.Wikipedia ? $"https://en.wikipedia.org/wiki/{dto.SourceId}" : dto.SourceId
                };
                await _repository.CreateSourceAsync(source);
            }
        }

        if (dto.Type != ActivityType.Explore && technicalSourceId == null && string.IsNullOrEmpty(dto.UserSessionId))
        {
            throw new ArgumentException($"Activity of type {dto.Type} requires a SourceId.");
        }

        UserSession? session = null;
        if (!string.IsNullOrEmpty(dto.UserSessionId))
        {
            session = await _repository.GetSessionByIdAsync(userId, dto.UserSessionId);
            if (session != null && session.TargetSourceId == null && technicalSourceId != null)
            {
                session.TargetSourceId = technicalSourceId;
                await _repository.UpdateSessionAsync(session);
                
                var sessionActivities = await _repository.GetAllAsync(userId); 
                foreach (var existingAct in sessionActivities.Where(a => a.UserSessionId == session.Id))
                {
                    existingAct.SourceId = technicalSourceId;
                    await _repository.UpdateAsync(existingAct);
                }
            }
        }

        if (session == null)
        {
            if (technicalSourceId != null)
            {
                session = await _repository.GetLastActiveSessionAsync(userId, technicalSourceId);
            }
            
            if (session == null)
            {
                session = new UserSession
                {
                    UserId = userId,
                    TargetSourceId = technicalSourceId,
                    StartedAt = dto.SessionDateStart,
                    Status = SessionStatus.Active
                };
                await _repository.CreateSessionAsync(session);
            }
        }

        if (dto.Type != ActivityType.Explore && session.TargetSourceId == null)
        {
            throw new InvalidOperationException($"Cannot add {dto.Type} activity to a session without a source.");
        }

        double? scorePercentage = null;
        if (dto.Type == ActivityType.Quiz && dto.QuestionCount.HasValue && dto.QuestionCount.Value > 0)
        {
            scorePercentage = (double)(dto.Score ?? 0) / dto.QuestionCount.Value * 100.0;
        }

        bool isNewBest = false;
        bool isBaseline = false;
        if (dto.Type == ActivityType.Quiz && technicalSourceId != null)
        {
            var history = await _repository.GetAllForContentAsync(userId, technicalSourceId);
            var quizzes = history.Where(h => h.Type == ActivityType.Quiz).ToList();
            isBaseline = !quizzes.Any();

            if (scorePercentage.HasValue)
            {
                var prevBest = quizzes.Where(h => h.ScorePercentage.HasValue).Max(h => (double?)h.ScorePercentage) ?? -1.0;
                isNewBest = scorePercentage.Value > prevBest;
            }
        }

        var activity = new UserActivity
        {
            UserId = userId,
            UserSessionId = session.Id,
            SourceId = session.TargetSourceId,
            Type = dto.Type,
            Title = dto.Title,
            Description = string.IsNullOrEmpty(dto.Description) ? $"Activity on {dto.Title}" : dto.Description,
            SessionDateStart = dto.SessionDateStart,
            SessionDateEnd = dto.SessionDateEnd,
            ExploreDurationSeconds = dto.ExploreDurationSeconds,
            ReadDurationSeconds = dto.ReadDurationSeconds,
            QuizDurationSeconds = dto.QuizDurationSeconds,
            Score = dto.Score ?? 0,
            QuestionCount = dto.QuestionCount ?? 0,
            ScorePercentage = scorePercentage,
            IsNewBestScore = isNewBest,
            IsBaseline = isBaseline,
            IsCompleted = true,
            LlmModelName = dto.LlmModelName,
            LlmVersion = dto.LlmVersion,
            Payload = dto.Payload,
            BacklogAddsCount = dto.BacklogAddsCount
        };

        await _repository.CreateAsync(activity);

        if (!string.IsNullOrEmpty(dto.OriginExploreId) && dto.Type == ActivityType.Read)
        {
            var explore = await _repository.GetByIdAsync(userId, dto.OriginExploreId);
            if (explore != null)
            {
                explore.ResultingReadActivityId = activity.Id;
                await _repository.UpdateAsync(explore);
            }
        }

        return activity;
    }

    public async Task<UserStatisticsDto> GetStatisticsAsync(string userId) => await _repository.GetStatisticsAsync(userId);
    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365) => await _repository.GetActivityCalendarAsync(userId, days);
    public async Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10) => await _repository.GetTopScoresAsync(userId, limit);
    public async Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId) => await _repository.GetByIdAsync(userId, activityId);
    public async Task DeleteActivityAsync(string userId, string activityId) => await _repository.DeleteAsync(userId, activityId);

    public async Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(string userId) 
    {
        var activities = await _repository.GetAllAsync(userId);
        return ApplyUniversalRules(activities.Select(a => MapToDto(a, false)));
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllForContentAsync(string userId, string sourceId)
    {
        var activities = await _repository.GetAllForContentAsync(userId, sourceId);
        return ApplyUniversalRules(activities.Select(a => MapToDto(a, false)));
    }

    public async Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException($"Activity {activityId} not found");

        if (dto.Score.HasValue) activity.Score = dto.Score.Value;
        if (dto.QuestionCount.HasValue) activity.QuestionCount = dto.QuestionCount.Value;
        if (dto.ReadDurationSeconds.HasValue) activity.ReadDurationSeconds = dto.ReadDurationSeconds.Value;
        if (dto.ExploreDurationSeconds.HasValue) activity.ExploreDurationSeconds = dto.ExploreDurationSeconds.Value;
        if (dto.QuizDurationSeconds.HasValue) activity.QuizDurationSeconds = dto.QuizDurationSeconds.Value;
        if (dto.SessionDateEnd.HasValue) activity.SessionDateEnd = dto.SessionDateEnd.Value;
        if (dto.IsCompleted.HasValue) activity.IsCompleted = dto.IsCompleted.Value;

        if (dto.Score.HasValue || dto.QuestionCount.HasValue)
        {
            if (activity.QuestionCount > 0)
            {
                activity.ScorePercentage = (double)activity.Score / activity.QuestionCount * 100.0;
                var session = await _repository.GetSessionByIdAsync(userId, activity.UserSessionId);
                var sourceId = session?.TargetSourceId ?? string.Empty;
                var history = await _repository.GetAllForContentAsync(userId, sourceId);
                var prevBest = history.Where(h => h.Id != activityId && h.ScorePercentage.HasValue).Max(h => (double?)h.ScorePercentage) ?? -1.0;
                activity.IsNewBestScore = activity.ScorePercentage.Value > prevBest;
            }
        }

        if (!string.IsNullOrEmpty(dto.LlmModelName)) activity.LlmModelName = dto.LlmModelName;
        if (!string.IsNullOrEmpty(dto.LlmVersion)) activity.LlmVersion = dto.LlmVersion;
        if (!string.IsNullOrEmpty(dto.ResultingReadActivityId)) activity.ResultingReadActivityId = dto.ResultingReadActivityId;
        if (dto.BacklogAddsCount.HasValue) activity.BacklogAddsCount = dto.BacklogAddsCount.Value;

        await _repository.UpdateAsync(activity);
        return activity;
    }

    private UserActivityDto MapToDto(UserActivity a, bool isTracked)
    {
        var source = a.Source ?? a.UserSession?.TargetSource;
        return new UserActivityDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserSessionId = a.UserSessionId,
            Title = a.Title,
            Description = a.Description,
            SourceId = source?.ExternalId ?? string.Empty,
            SourceType = source?.Type ?? SourceType.Custom,
            SourceHash = source?.Id ?? string.Empty,
            Type = a.Type,
            SessionDateStart = a.SessionDateStart,
            SessionDateEnd = a.SessionDateEnd,
            ExploreDurationSeconds = a.ExploreDurationSeconds,
            ReadDurationSeconds = a.ReadDurationSeconds,
            QuizDurationSeconds = a.QuizDurationSeconds,
            TotalDurationSeconds = a.TotalDurationSeconds,
            Score = a.Score,
            QuestionCount = a.QuestionCount,
            ScorePercentage = a.ScorePercentage,
            IsNewBestScore = a.IsNewBestScore,
            IsBaseline = a.IsBaseline,
            IsCurrentBest = false, // Will be set by ApplyUniversalRules
            IsCompleted = a.IsCompleted,
            LlmModelName = a.LlmModelName,
            LlmVersion = a.LlmVersion,
            IsTracked = isTracked,
            Payload = a.Payload,
            ResultingReadActivityId = a.ResultingReadActivityId,
            BacklogAddsCount = a.BacklogAddsCount
        };
    }

    private IEnumerable<UserActivityDto> ApplyUniversalRules(IEnumerable<UserActivityDto> dtos)
    {
        var list = dtos.ToList();
        var sourceGroups = list.GroupBy(d => d.SourceHash).Where(g => !string.IsNullOrEmpty(g.Key));

        foreach (var group in sourceGroups)
        {
            var quizzes = group.Where(d => d.Type == ActivityType.Quiz && d.ScorePercentage.HasValue)
                               .OrderBy(d => d.SessionDateStart)
                               .ToList();

            if (!quizzes.Any())
            {
                // Ensure no badges for non-quizzes (especially READ)
                foreach (var item in group)
                {
                    item.IsBaseline = false;
                    item.IsNewBestScore = false;
                    item.IsCurrentBest = false;
                }
                continue;
            }

            // 1. Identify Current Best score (overall) for the Trophies
            var currentMaxOverall = quizzes.Max(q => q.ScorePercentage!.Value);

            foreach (var item in group)
            {
                if (item.Type == ActivityType.Quiz && item.ScorePercentage.HasValue)
                {
                    // IsBaseline and IsNewBestScore are NOT recalculated. 
                    // They are snapshots of the state at creation time.
                    
                    // IsCurrentBest: True if this matches the maximum score ever achieved for this topic
                    item.IsCurrentBest = Math.Abs(item.ScorePercentage.Value - currentMaxOverall) < 0.01;
                }
                else
                {
                    // Universal rule: Only Quizzes get score-related badges
                    item.IsBaseline = false;
                    item.IsNewBestScore = false;
                    item.IsCurrentBest = false;
                }
            }
        }

        return list;
    }
}
