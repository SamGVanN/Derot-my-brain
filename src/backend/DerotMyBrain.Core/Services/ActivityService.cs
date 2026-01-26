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
    private readonly IUserFocusService _userFocusService;
    private readonly IEnumerable<IContentSource> _contentSources;
    private readonly ILlmService _llmService;
    private readonly IJsonSerializer _jsonSerializer;

    public ActivityService(
        IActivityRepository repository,
        IUserFocusService userFocusService,
        IEnumerable<IContentSource> contentSources,
        ILlmService llmService,
        IJsonSerializer jsonSerializer)
    {
        _repository = repository;
        _userFocusService = userFocusService;
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
            SourceId = sourceId, // May be null now
            SourceType = sourceType,
            Type = ActivityType.Explore,
            SessionDateStart = DateTime.UtcNow
        };

        return await CreateActivityAsync(userId, dto);
    }

    public async Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceId, SourceType sourceType, string? originExploreId = null, int? backlogAddsCount = null, int? exploreDurationSeconds = null)
    {
        // 1. Resolve source and fetch content
        var sourceName = sourceType == SourceType.Wikipedia ? "Wikipedia" : "File"; 
        var source = _contentSources.FirstOrDefault(s => s.CanHandle(sourceName));
        if (source == null) throw new InvalidOperationException($"Content source for {sourceType} not found.");

        var content = await source.GetContentAsync(sourceId ?? title);

        // 2. Create activity
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
        
        // Ensure ArticleContent is set
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

        return new QuizDto
        {
            Questions = questions
        };
    }

    public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
    {
        // 1. Resolve Source (Deduplication)
        string? technicalSourceId = null;
        Source? source = null;

        if (!string.IsNullOrEmpty(dto.SourceId))
        {
            technicalSourceId = SourceHasher.GenerateHash(dto.SourceType, dto.SourceId);
            source = await _repository.GetSourceByIdAsync(technicalSourceId);
            if (source == null)
            {
                source = new Source
                {
                    Id = technicalSourceId,
                    Type = dto.SourceType,
                    ExternalId = dto.SourceId,
                    DisplayTitle = dto.Title,
                    Url = dto.SourceType == SourceType.Wikipedia ? $"https://en.wikipedia.org/wiki/{dto.SourceId}" : dto.SourceId
                };
                await _repository.CreateSourceAsync(source);
            }
        }

        // Validation: Read and Quiz MUST have a source (either provided or in session)
        if (dto.Type != ActivityType.Explore && technicalSourceId == null && string.IsNullOrEmpty(dto.UserSessionId))
        {
            throw new ArgumentException($"Activity of type {dto.Type} requires a SourceId.");
        }

        // 2. Resolve/Create Session
        UserSession? session = null;
        if (!string.IsNullOrEmpty(dto.UserSessionId))
        {
            session = await _repository.GetSessionByIdAsync(userId, dto.UserSessionId);
            
            // Session Promotion Logic: 
            // If the session exists but has no source, and this new activity DOES have a source, "promote" the session.
            if (session != null && session.SourceId == null && technicalSourceId != null)
            {
                session.SourceId = technicalSourceId;
                await _repository.UpdateSessionAsync(session);
            }
        }

        if (session == null)
        {
            // Try to find a last active session for this source (if we have one)
            if (technicalSourceId != null)
            {
                session = await _repository.GetLastActiveSessionAsync(userId, technicalSourceId);
            }
            
            if (session == null)
            {
                session = new UserSession
                {
                    UserId = userId,
                    SourceId = technicalSourceId, // Might be null for Explore
                    StartedAt = dto.SessionDateStart,
                    Status = SessionStatus.Active
                };
                await _repository.CreateSessionAsync(session);
            }
        }

        // Final Validation for Read/Quiz: Even with session, we need a source ID at this point
        if (dto.Type != ActivityType.Explore && session.SourceId == null)
        {
            // This happens if we try to add a Read/Quiz to a session that still doesn't have a source
            // and the DTO didn't provide one either.
            throw new InvalidOperationException($"Cannot add {dto.Type} activity to a session without a source.");
        }

        // Calculate Score Percentage
        double? scorePercentage = null;
        if (dto.Type == ActivityType.Quiz && dto.QuestionCount.HasValue && dto.QuestionCount.Value > 0)
        {
            scorePercentage = (double)(dto.Score ?? 0) / dto.QuestionCount.Value * 100.0;
        }

        // Check for New Best Score and Baseline
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

        // Link Explore result if applicable
        if (!string.IsNullOrEmpty(dto.OriginExploreId) && dto.Type == ActivityType.Read)
        {
            var explore = await _repository.GetByIdAsync(userId, dto.OriginExploreId);
            if (explore != null)
            {
                explore.ResultingReadActivityId = activity.Id;
                await _repository.UpdateAsync(explore);
            }
        }

        // Update focus stats (only if we have a source)
        var sourceIdForStats = session.SourceId;
        if (sourceIdForStats != null)
        {
            await _userFocusService.UpdateStatsAsync(userId, sourceIdForStats, activity);
        }

        return activity;
    }

    public async Task<UserStatisticsDto> GetStatisticsAsync(string userId) => await _repository.GetStatisticsAsync(userId);
    
    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365) 
        => await _repository.GetActivityCalendarAsync(userId, days);
        
    public async Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10) 
        => await _repository.GetTopScoresAsync(userId, limit);

    public async Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId) 
        => await _repository.GetByIdAsync(userId, activityId);

    public async Task DeleteActivityAsync(string userId, string activityId) 
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) return;

        // Fetch session to get sourceId
        var session = await _repository.GetSessionByIdAsync(userId, activity.UserSessionId);
        if (session == null) return;

        var sourceId = session.SourceId;
        await _repository.DeleteAsync(userId, activityId);
        
        if (sourceId != null) await _userFocusService.RebuildStatsAsync(userId, sourceId);
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(string userId) 
    {
        var activities = await _repository.GetAllAsync(userId);
        var trackedSourceIds = (await _userFocusService.GetAllFocusesAsync(userId))
            .Select(t => t.SourceId)
            .ToHashSet();

        return activities.Select(a => MapToDto(a, trackedSourceIds.Contains(a.UserSession.SourceId)));
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllForContentAsync(string userId, string sourceId)
    {
        var activities = await _repository.GetAllForContentAsync(userId, sourceId);
        var isTracked = await _userFocusService.GetFocusAsync(userId, sourceId) != null;
        
        return activities.Select(a => MapToDto(a, isTracked));
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

        // Recalculate score percentage if updated
        if (dto.Score.HasValue || dto.QuestionCount.HasValue)
        {
            if (activity.QuestionCount > 0)
            {
                activity.ScorePercentage = (double)activity.Score / activity.QuestionCount * 100.0;
                
                // Fetch sourceId through session
                var session = await _repository.GetSessionByIdAsync(userId, activity.UserSessionId);
                var sourceId = session?.SourceId ?? string.Empty;

                var history = await _repository.GetAllForContentAsync(userId, sourceId);
                var prevBest = history.Where(h => h.Id != activityId && h.ScorePercentage.HasValue)
                                     .Max(h => (double?)h.ScorePercentage) ?? -1.0;
                activity.IsNewBestScore = activity.ScorePercentage.Value > prevBest;
            }
        }

        if (!string.IsNullOrEmpty(dto.LlmModelName)) activity.LlmModelName = dto.LlmModelName;
        if (!string.IsNullOrEmpty(dto.LlmVersion)) activity.LlmVersion = dto.LlmVersion;
        if (!string.IsNullOrEmpty(dto.ResultingReadActivityId)) activity.ResultingReadActivityId = dto.ResultingReadActivityId;
        if (dto.BacklogAddsCount.HasValue) activity.BacklogAddsCount = dto.BacklogAddsCount.Value;

        await _repository.UpdateAsync(activity);
        
        var sourceIdForStats = (await _repository.GetSessionByIdAsync(userId, activity.UserSessionId))?.SourceId;
        if (sourceIdForStats != null) await _userFocusService.RebuildStatsAsync(userId, sourceIdForStats);

        return activity;
    }

    private UserActivityDto MapToDto(UserActivity a, bool isTracked)
    {
        // Use UserSession/Source info for the DTO
        var source = a.UserSession?.Source;

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
            IsCompleted = a.IsCompleted,
            LlmModelName = a.LlmModelName,
            LlmVersion = a.LlmVersion,
            IsTracked = isTracked,
            Payload = a.Payload,
            ResultingReadActivityId = a.ResultingReadActivityId,
            BacklogAddsCount = a.BacklogAddsCount
        };
    }
}
