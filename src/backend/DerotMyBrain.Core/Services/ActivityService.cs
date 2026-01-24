using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using System.Text.Json;

namespace DerotMyBrain.Core.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _repository;
    private readonly ITrackedTopicService _trackedTopicService;
    private readonly IEnumerable<IContentSource> _contentSources;
    private readonly ILlmService _llmService;

    public ActivityService(
        IActivityRepository repository,
        ITrackedTopicService trackedTopicService,
        IEnumerable<IContentSource> contentSources,
        ILlmService llmService)
    {
        _repository = repository;
        _trackedTopicService = trackedTopicService;
        _contentSources = contentSources;
        _llmService = llmService;
    }

    public async Task<ContentResult> StartReadingAsync(string userId, StartActivityRequest request)
    {
        // 1. Resolve Strategy
        var source = _contentSources.FirstOrDefault(s => s.CanHandle(request.Type));
        
        if (source == null)
        {
             source = _contentSources.FirstOrDefault(s => s.CanHandle("Wikipedia")); 
        }
        
        if (source == null) throw new InvalidOperationException("No suitable content source found.");

        // 2. Fetch Content
        var content = await source.GetContentAsync(request.Filter); 

        // 3. Create Activity
        var activity = new UserActivity
        {
            UserId = userId,
            Type = "Reading",
            Title = content.Title,
            Description = "Reading article: " + content.Title,
            SourceUrl = content.SourceUrl,
            ContentSourceType = content.SourceType,
            ArticleContent = content.TextContent, 
            LastAttemptDate = DateTime.UtcNow,
            IsTracked = false
        };

        await _repository.CreateAsync(activity);

        return content;
    }

    public async Task<QuizDto> GenerateQuizAsync(string userId, string activityId)
    {
        // 1. Get Activity
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException("Activity not found");

        // 2. Get Content
        string textToProcess = activity.ArticleContent;
        if (string.IsNullOrEmpty(textToProcess)) throw new InvalidOperationException("No content available to generate quiz.");

        // 3. Generate Questions via LLM
        var questionsJson = await _llmService.GenerateQuestionsAsync(textToProcess);
        
        // Deserialize questions
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var questions = JsonSerializer.Deserialize<List<QuestionDto>>(questionsJson, options) ?? new List<QuestionDto>();

        // 4. Create separate Quiz Activity
        var quizActivity = new UserActivity
        {
            UserId = userId,
            Type = "Quiz_Pending", 
            Title = "Quiz: " + activity.Title,
            Description = "Generated quiz for " + activity.Title,
            SourceUrl = activity.SourceUrl,
            ArticleContent = textToProcess, 
            Payload = questionsJson, 
            LastAttemptDate = DateTime.UtcNow
        };
        
        await _repository.CreateAsync(quizActivity);

        return new QuizDto
        {
            Questions = questions
        };
    }

    public async Task<UserStatisticsDto> GetStatisticsAsync(string userId) => await _repository.GetStatisticsAsync(userId);
    
    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days = 365) 
        => await _repository.GetActivityCalendarAsync(userId, days);
        
    public async Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit = 10) 
        => await _repository.GetTopScoresAsync(userId, limit);

    public async Task<UserActivity?> GetActivityByIdAsync(string userId, string activityId) 
        => await _repository.GetByIdAsync(userId, activityId);

    public async Task DeleteActivityAsync(string userId, string activityId) 
        => await _repository.DeleteAsync(userId, activityId);

    public async Task<IEnumerable<UserActivity>> GetAllActivitiesAsync(string userId) 
        => await _repository.GetAllAsync(userId);

    public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
    {
        var activity = new UserActivity
        {
            UserId = userId,
            Type = dto.Type, // "Quiz", "Read"
            Title = dto.Title,
            Description = $"Activity on {dto.Title}",
            SourceUrl = dto.WikipediaUrl,
            IsTracked = true, // Default to tracked
            LastAttemptDate = DateTime.UtcNow,
            Score = dto.Score ?? 0,
            MaxScore = dto.TotalQuestions ?? 0
        };

        await _repository.CreateAsync(activity);

        // Update topic stats if relevant. Fixed: passing activity.
        await _trackedTopicService.UpdateStatsAsync(userId, dto.Title, activity);

        return activity;
    }

    public async Task<UserActivity> UpdateActivityAsync(string userId, string activityId, UpdateActivityDto dto)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null)
            throw new KeyNotFoundException($"Activity {activityId} not found");

        if (dto.Score.HasValue) activity.Score = dto.Score.Value;
        if (dto.TotalQuestions.HasValue) activity.MaxScore = dto.TotalQuestions.Value;
        if (!string.IsNullOrEmpty(dto.LlmModelName)) activity.LlmModelName = dto.LlmModelName;
        if (!string.IsNullOrEmpty(dto.LlmVersion)) activity.LlmVersion = dto.LlmVersion;

        await _repository.UpdateAsync(activity);
        
        // Update tracked topic stats if applicable
        if (activity.IsTracked)
        {
            // Fixed: passing activity
            await _trackedTopicService.UpdateStatsAsync(userId, activity.Title, activity);
        }

        return activity;
    }

    public async Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic)
        => await _repository.GetAllForTopicAsync(userId, topic);
}
