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

    public async Task<ContentResult> StartReadingAsync(string userId, StartActivityRequest request)
    {
        // 1. Resolve Strategy
        var source = _contentSources.FirstOrDefault(s => s.CanHandle(request.Type));
        if (source == null) throw new InvalidOperationException("No suitable content source found.");

        // 2. Fetch Content
        var content = await source.GetContentAsync(request.Filter); 

        // 3. Create Activity (Initial Read)
        // Map string SourceType from ContentResult to Enum
        Enum.TryParse<SourceType>(content.SourceType, true, out var sourceType);
        if (sourceType == 0) sourceType = SourceType.Wikipedia;

        var sourceHash = SourceHasher.GenerateHash(sourceType, content.SourceUrl);
        var activity = new UserActivity
        {
            UserId = userId,
            Type = ActivityType.Read,
            Title = content.Title,
            Description = "Reading article: " + content.Title,
            SourceId = content.SourceUrl,
            SourceType = sourceType,
            SourceHash = sourceHash,
            ArticleContent = content.TextContent, 
            SessionDateStart = DateTime.UtcNow,
            SessionDateEnd = null, // Still reading
            ReadDurationSeconds = 0 
        };

        await _repository.CreateAsync(activity);

        return content;
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
        var sourceHash = SourceHasher.GenerateHash(dto.SourceType, dto.SourceId);
        
        // Calculate Score Percentage
        double? scorePercentage = null;
        if (dto.Type == ActivityType.Quiz && dto.QuestionCount.HasValue && dto.QuestionCount.Value > 0)
        {
            scorePercentage = (double)(dto.Score ?? 0) / dto.QuestionCount.Value * 100.0;
        }

        // Check for New Best Score and Baseline
        bool isNewBest = false;
        bool isBaseline = false;
        if (dto.Type == ActivityType.Quiz)
        {
            var history = await _repository.GetAllForContentAsync(userId, sourceHash);
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
            Type = dto.Type,
            Title = dto.Title,
            Description = string.IsNullOrEmpty(dto.Description) ? $"Activity on {dto.Title}" : dto.Description,
            SourceId = dto.SourceId,
            SourceType = dto.SourceType,
            SourceHash = sourceHash,
            SessionDateStart = dto.SessionDateStart,
            SessionDateEnd = dto.SessionDateEnd,
            ReadDurationSeconds = dto.ReadDurationSeconds,
            QuizDurationSeconds = dto.QuizDurationSeconds,
            Score = dto.Score ?? 0,
            QuestionCount = dto.QuestionCount ?? 0,
            ScorePercentage = scorePercentage,
            IsNewBestScore = isNewBest,
            IsBaseline = isBaseline,
            IsCompleted = true, // If created via DTO, usually it's a finished session
            LlmModelName = dto.LlmModelName,
            LlmVersion = dto.LlmVersion,
            Payload = dto.Payload
            ,
            // Support Explore-specific metadata if provided
            BacklogAddsCount = dto.BacklogAddsCount
        };

        await _repository.CreateAsync(activity);

        // If this Read was originated from an Explore session, link them.
        if (!string.IsNullOrEmpty(dto.OriginExploreId) && dto.Type == ActivityType.Read)
        {
            try
            {
                var explore = await _repository.GetByIdAsync(userId, dto.OriginExploreId);
                if (explore != null)
                {
                    explore.ResultingReadActivityId = activity.Id;
                    await _repository.UpdateAsync(explore);
                }
            }
            catch
            {
                // Do not fail the creation of the Read activity if linking fails;
                // logging could be added here in real implementation.
            }
        }

        // Update focus stats
        await _userFocusService.UpdateStatsAsync(userId, sourceHash, activity);

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

        var sourceHash = activity.SourceHash;
        await _repository.DeleteAsync(userId, activityId);
        
        await _userFocusService.RebuildStatsAsync(userId, sourceHash);
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(string userId) 
    {
        var activities = await _repository.GetAllAsync(userId);
        var trackedHashes = (await _userFocusService.GetAllFocusesAsync(userId))
            .Select(t => t.SourceHash)
            .ToHashSet();

        return activities.Select(a => MapToDto(a, trackedHashes.Contains(a.SourceHash)));
    }

    public async Task<IEnumerable<UserActivityDto>> GetAllForContentAsync(string userId, string sourceHash)
    {
        var activities = await _repository.GetAllForContentAsync(userId, sourceHash);
        var isTracked = await _userFocusService.GetFocusAsync(userId, sourceHash) != null;
        
        return activities.Select(a => MapToDto(a, isTracked));
    }

    public async Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException($"Activity {activityId} not found");

        if (dto.Score.HasValue) activity.Score = dto.Score.Value;
        if (dto.QuestionCount.HasValue) activity.QuestionCount = dto.QuestionCount.Value;
        if (dto.ReadDurationSeconds.HasValue) activity.ReadDurationSeconds = dto.ReadDurationSeconds.Value;
        if (dto.QuizDurationSeconds.HasValue) activity.QuizDurationSeconds = dto.QuizDurationSeconds.Value;
        if (dto.SessionDateEnd.HasValue) activity.SessionDateEnd = dto.SessionDateEnd.Value;
        if (dto.IsCompleted.HasValue) activity.IsCompleted = dto.IsCompleted.Value;

        // Recalculate score percentage if updated
        if (dto.Score.HasValue || dto.QuestionCount.HasValue)
        {
            if (activity.QuestionCount > 0)
            {
                activity.ScorePercentage = (double)activity.Score / activity.QuestionCount * 100.0;
                
                var history = await _repository.GetAllForContentAsync(userId, activity.SourceHash);
                var prevBest = history.Where(h => h.Id != activityId && h.ScorePercentage.HasValue)
                                     .Max(h => (double?)h.ScorePercentage) ?? -1.0;
                activity.IsNewBestScore = activity.ScorePercentage.Value > prevBest;
            }
        }

        if (!string.IsNullOrEmpty(dto.LlmModelName)) activity.LlmModelName = dto.LlmModelName;
        if (!string.IsNullOrEmpty(dto.LlmVersion)) activity.LlmVersion = dto.LlmVersion;

        // Apply Explore linkage updates when provided
        if (!string.IsNullOrEmpty(dto.ResultingReadActivityId))
        {
            activity.ResultingReadActivityId = dto.ResultingReadActivityId;
        }

        if (dto.BacklogAddsCount.HasValue)
        {
            activity.BacklogAddsCount = dto.BacklogAddsCount.Value;
        }

        await _repository.UpdateAsync(activity);
        
        await _userFocusService.RebuildStatsAsync(userId, activity.SourceHash);

        return activity;
    }

    private UserActivityDto MapToDto(UserActivity a, bool isTracked)
    {
        return new UserActivityDto
        {
            Id = a.Id,
            UserId = a.UserId,
            Title = a.Title,
            Description = a.Description,
            SourceId = a.SourceId,
            SourceType = a.SourceType,
            SourceHash = a.SourceHash,
            Type = a.Type,
            SessionDateStart = a.SessionDateStart,
            SessionDateEnd = a.SessionDateEnd,
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
            Payload = a.Payload
            ,
            ResultingReadActivityId = a.ResultingReadActivityId,
            BacklogAddsCount = a.BacklogAddsCount
        };
    }
}
