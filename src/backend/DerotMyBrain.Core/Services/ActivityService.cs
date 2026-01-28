using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _repository;
    private readonly IEnumerable<IContentSource> _contentSources;
    private readonly IWikipediaService _wikipediaService;
    private readonly ILlmService _llmService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        IActivityRepository repository,
        IEnumerable<IContentSource> contentSources,
        IWikipediaService wikipediaService,
        ILlmService llmService,
        IJsonSerializer jsonSerializer,
        ILogger<ActivityService> logger)
    {
        _repository = repository;
        _contentSources = contentSources;
        _wikipediaService = wikipediaService;
        _llmService = llmService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<IEnumerable<WikipediaArticleDto>> GetExploreArticlesAsync(string userId, int count = 6)
    {
        // For now, we just fetch random articles. 
        // In the future, we could use user interests/categories.
        return await _wikipediaService.GetDiscoveryArticlesAsync(count);
    }

    public async Task<UserActivity> ExploreAsync(string userId, string? title = null, string? sourceId = null, SourceType sourceType = SourceType.Custom, string? sessionId = null)
    {
        var dto = new CreateActivityDto
        {
            UserSessionId = sessionId,
            Title = title ?? "Exploration",
            Description = "Exploration session",
            SourceId = sourceId, 
            SourceType = sourceType,
            Type = ActivityType.Explore,
            SessionDateStart = DateTime.UtcNow
        };

        return await CreateActivityAsync(userId, dto);
    }

    public async Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceId, SourceType sourceType, string? originExploreId = null, int? backlogAddsCount = null, int? refreshCount = null, int? exploreDurationSeconds = null)
    {
        var sourceName = sourceType == SourceType.Wikipedia ? "Wikipedia" : "File"; 
        var contentSource = _contentSources.FirstOrDefault(s => s.CanHandle(sourceName));
        if (contentSource == null) throw new InvalidOperationException($"Content source for {sourceType} not found.");

        // If sourceId is provided, check if it's a technical ID (GUID or Hash) first
        string contentIdentifier = sourceId ?? title;
        if (!string.IsNullOrEmpty(sourceId))
        {
            // If it's a URL, use it directly as identifier
            if (sourceId.Contains("wikipedia.org") || sourceId.StartsWith("http"))
            {
                contentIdentifier = sourceId;
            }
            else 
            {
                var existingSource = await _repository.GetSourceByIdAsync(sourceId);
                if (existingSource != null)
                {
                    contentIdentifier = existingSource.ExternalId;
                }
                else if (sourceId.Length == 64 && !string.IsNullOrEmpty(title))
                {
                    // If it looks like a hash (64 chars) but we don't know it, 
                    // it might be a virtual ID from an explore session.
                    // Fall back to title to be safe for Wikipedia.
                    _logger.LogWarning("SourceId {SourceId} not found in DB and looks like a hash. Falling back to title: {Title}", sourceId, title);
                    contentIdentifier = title;
                }
            }
        }

        ContentResult content;
        try 
        {
            content = await contentSource.GetContentAsync(contentIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get content from {SourceType} for {Identifier}", sourceType, contentIdentifier);
            throw;
        }

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
            RefreshCount = refreshCount,
            DurationSeconds = 0 // Initial read duration is 0
        };

        if (!string.IsNullOrEmpty(originExploreId))
        {
            var exploreActivity = await _repository.GetByIdAsync(userId, originExploreId);
            if (exploreActivity != null)
            {
                dto.UserSessionId = exploreActivity.UserSessionId;
                
                // Duration update for explore activity still needs to happen here or be passed to DTO
                if (exploreDurationSeconds.HasValue) 
                {
                    exploreActivity.DurationSeconds = exploreDurationSeconds.Value;
                    await _repository.UpdateAsync(exploreActivity);
                }
            }
        }

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
        if (technicalSourceId != null)
        {
            var source = await _repository.GetSourceByIdAsync(technicalSourceId);
            if (source == null)
            {
                _logger.LogInformation("Creating new source with ID {SourceId} and ExternalId {ExternalId}", technicalSourceId, dto.SourceId);
                source = new Source
                {
                    Id = technicalSourceId,
                    UserId = userId,
                    Type = dto.SourceType,
                    ExternalId = dto.SourceId,
                    DisplayTitle = dto.Title,
                    IsTracked = false
                };
                await _repository.CreateSourceAsync(source);
            }
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
                _logger.LogInformation("Updating session {SessionId} TargetSourceId to {SourceId}", session.Id, technicalSourceId);
                session.TargetSourceId = technicalSourceId;
                await _repository.UpdateSessionAsync(session);
                
                // Update all existing activities in this session to point to the now-known technical source
                foreach (var existingAct in session.Activities)
                {
                    if (string.IsNullOrEmpty(existingAct.SourceId))
                    {
                        _logger.LogInformation("Updating activity {ActivityId} SourceId to {SourceId}", existingAct.Id, technicalSourceId);
                        existingAct.SourceId = technicalSourceId;
                        await _repository.UpdateAsync(existingAct);
                    }
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

        // Ensure OnlineResource exists for external sources (Wikipedia, etc.)
        if (technicalSourceId != null && (dto.SourceType == SourceType.Wikipedia || dto.SourceId?.StartsWith("http") == true))
        {
            var existingResource = await _repository.GetOnlineResourceBySourceIdAsync(technicalSourceId);
            if (existingResource == null)
            {
                var url = dto.SourceId ?? string.Empty;
                if (dto.SourceType == SourceType.Wikipedia && !url.StartsWith("http"))
                {
                    url = $"https://en.wikipedia.org/wiki/{dto.SourceId}";
                }

                if (!string.IsNullOrEmpty(url))
                {
                    var onlineResource = new OnlineResource
                    {
                        UserId = userId,
                        SourceId = technicalSourceId,
                        URL = url,
                        Title = dto.Title,
                        Provider = dto.SourceType == SourceType.Wikipedia ? "Wikipedia" : null,
                        SavedAt = DateTime.UtcNow
                    };
                    await _repository.CreateOnlineResourceAsync(onlineResource);
                }
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
            DurationSeconds = dto.DurationSeconds ?? 0,
            Score = dto.Score ?? 0,
            QuestionCount = dto.QuestionCount ?? 0,
            ScorePercentage = scorePercentage,
            IsNewBestScore = isNewBest,
            IsBaseline = isBaseline,
            IsCompleted = true,
            LlmModelName = dto.LlmModelName,
            LlmVersion = dto.LlmVersion,
            Payload = dto.Payload,
            OriginExploreId = dto.OriginExploreId,
            BacklogAddsCount = dto.BacklogAddsCount,
            RefreshCount = dto.RefreshCount ?? 0
        };

        await _repository.CreateAsync(activity);

        if (!string.IsNullOrEmpty(dto.OriginExploreId) && dto.Type == ActivityType.Read)
        {
            var explore = await _repository.GetByIdAsync(userId, dto.OriginExploreId);
            if (explore != null)
            {
                explore.ResultingReadActivityId = activity.Id;
                
                // Persist the stats from the exploration phase to the explore activity record
                if (dto.BacklogAddsCount.HasValue) explore.BacklogAddsCount = dto.BacklogAddsCount.Value;
                if (dto.RefreshCount.HasValue) explore.RefreshCount = dto.RefreshCount.Value;
                
                explore.IsCompleted = true;
                explore.SessionDateEnd = DateTime.UtcNow;
                
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

    public async Task StopSessionAsync(string userId, string sessionId)
    {
        var session = await _repository.GetSessionByIdAsync(userId, sessionId);
        if (session == null) return;

        session.Status = SessionStatus.Stopped;
        session.EndedAt = DateTime.UtcNow;
        await _repository.UpdateSessionAsync(session);

        _logger.LogInformation("Session {SessionId} stopped for user {UserId}", sessionId, userId);
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(string userId) 
    {
        var activities = await _repository.GetAllAsync(userId);
        var dtos = activities.OrderByDescending(a => a.SessionDateStart).Select(a => MapToDto(a, a.Source?.IsTracked ?? false));
        return ApplyUniversalRules(dtos);
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllForContentAsync(string userId, string sourceId)
    {
        var activities = await _repository.GetAllForContentAsync(userId, sourceId);
        var dtos = activities.OrderByDescending(a => a.SessionDateStart).Select(a => MapToDto(a, a.Source?.IsTracked ?? false));
        return ApplyUniversalRules(dtos);
    }

    public async Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException($"Activity {activityId} not found");

        if (dto.Score.HasValue) activity.Score = dto.Score.Value;
        if (dto.QuestionCount.HasValue) activity.QuestionCount = dto.QuestionCount.Value;
        if (dto.DurationSeconds.HasValue) activity.DurationSeconds = dto.DurationSeconds.Value; // New standard
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
        if (dto.RefreshCount.HasValue) activity.RefreshCount = dto.RefreshCount.Value;

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
            SourceId = source?.Id ?? string.Empty,
            ExternalId = source?.ExternalId ?? string.Empty,
            SourceType = source?.Type ?? SourceType.Custom,
            DisplayTitle = source?.DisplayTitle ?? string.Empty,
            Url = source?.OnlineResource?.URL ?? (source?.Type == SourceType.Wikipedia ? $"https://en.wikipedia.org/wiki/{source.ExternalId}" : string.Empty),
            Type = a.Type,
            SessionDateStart = a.SessionDateStart,
            SessionDateEnd = a.SessionDateEnd,
            DurationSeconds = a.DurationSeconds,
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
            ArticleContent = a.ArticleContent,
            Payload = a.Payload,
            ResultingReadActivityId = a.ResultingReadActivityId,
            ResultingReadSourceName = a.ResultingReadActivity?.Title,
            OriginExploreId = a.OriginExploreId,
            BacklogAddsCount = a.BacklogAddsCount,
            RefreshCount = a.RefreshCount
        };
    }

    private IEnumerable<UserActivityDto> ApplyUniversalRules(IEnumerable<UserActivityDto> dtos)
    {
        var list = dtos.ToList();
        var sourceGroups = list.GroupBy(d => d.SourceId).Where(g => !string.IsNullOrEmpty(g.Key));

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
