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
        var source = _contentSources.FirstOrDefault(s => s.CanHandle(request.Type)); // Or Filter depending on logic
        // If Type is "RandomWiki", pass "Random" to source?
        // Let's assume request.Type maps to source capabilities
        
        if (source == null)
        {
             // Fallback or Specific logic. 
             // For "RandomWiki", maybe WikipediaContentSource handles "RandomWiki" type string
             source = _contentSources.FirstOrDefault(s => s.CanHandle("Wikipedia")); 
        }
        
        if (source == null) throw new InvalidOperationException("No suitable content source found.");

        // 2. Fetch Content
        var content = await source.GetContentAsync(request.Filter); // Filter contains URL or Random params

        // 3. Create Activity
        var activity = new UserActivity
        {
            UserId = userId,
            Type = "Reading",
            Title = content.Title,
            Description = "Reading article: " + content.Title,
            SourceUrl = content.SourceUrl,
            ContentSourceType = content.SourceType,
            ArticleContent = content.TextContent, // Cache content
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
        if (string.IsNullOrEmpty(textToProcess) && !string.IsNullOrEmpty(activity.SourceUrl))
        {
            // Re-fetch logic if not cached (implement if needed)
            // For now assume cached
        }

        if (string.IsNullOrEmpty(textToProcess)) throw new InvalidOperationException("No content available to generate quiz.");

        // 3. Generate Questions via LLM
        var questionsJson = await _llmService.GenerateQuestionsAsync(textToProcess);
        
        // Deserialize questions
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var questions = JsonSerializer.Deserialize<List<QuestionDto>>(questionsJson, options) ?? new List<QuestionDto>();

        // 4. Create separate Quiz Activity (or just return questions?)
        // Let's create a *Pending* Quiz Activity so we can link the result later?
        // Or simpler: Just return questions. The frontend submits the result to CreateActivity (Type=Quiz).
        // BUT, we want to ensure the quiz matches the questions generated.
        // Storing questions in a new Activity allows us to validate answers securely later.
        
        var quizActivity = new UserActivity
        {
            UserId = userId,
            Type = "Quiz_Pending", // Or just Quiz, but mark Incomplete
            Title = "Quiz: " + activity.Title,
            SourceUrl = activity.SourceUrl,
            ArticleContent = textToProcess, // Keep content context
            Payload = questionsJson, // Store generated questions
            LastAttemptDate = DateTime.UtcNow
        };
        
        await _repository.CreateAsync(quizActivity);

        return new QuizDto
        {
            Questions = questions
        };
    }

    // Existing / Passthrough
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

}
