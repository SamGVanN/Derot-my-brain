using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace DerotMyBrain.Core.Services;

public class ActivityService : IActivityService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _quizLocks = new();
    private readonly IActivityRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IEnumerable<IContentSource> _contentSources;
    private readonly IWikipediaService _wikipediaService;
    private readonly IQuizService _quizService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        IActivityRepository repository,
        IUserRepository userRepository,
        IEnumerable<IContentSource> contentSources,
        IWikipediaService wikipediaService,
        IQuizService quizService,
        IJsonSerializer jsonSerializer,
        ILogger<ActivityService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _contentSources = contentSources;
        _wikipediaService = wikipediaService;
        _quizService = quizService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<IEnumerable<WikipediaArticleDto>> GetExploreArticlesAsync(string userId, int count = 6)
    {
        var language = "en";
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Preferences != null)
        {
            language = user.Preferences.Language;
        }

        _logger.LogInformation("Fetching explore articles for user {UserId} in language {Language}", userId, language);
        return await _wikipediaService.GetDiscoveryArticlesAsync(count, language);
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

    public async Task<UserActivity> ReadAsync(string userId, string title, string? language, string? sourceId, SourceType? sourceType, ActivityType activityType = ActivityType.Read, string? originExploreId = null, string? sessionId = null, int? backlogAddsCount = null, int? refreshCount = null, int? exploreDurationSeconds = null)
    {
        if (string.IsNullOrEmpty(sourceId))
             throw new ArgumentNullException(nameof(sourceId), "SourceId is required for Read activity.");

        Source? source = null;
        string? technicalSourceId = null;

        // 1. Try to resolve Source Entity by technical ID (if it's a GUID/Hash)
        source = await _repository.GetSourceByIdAsync(sourceId);

        if (source == null)
        {
            // 2. If not found, deduce type and technical ID
            SourceType effectiveType = sourceType ?? (sourceId.Contains("wikipedia.org") || !sourceId.Contains("-") ? SourceType.Wikipedia : SourceType.Document);
            
            if (effectiveType == SourceType.Wikipedia)
            {
                 technicalSourceId = SourceHasher.GenerateId(effectiveType, sourceId);
                 source = await _repository.GetSourceByIdAsync(technicalSourceId);
                 
                 if (source == null)
                 {
                     _logger.LogInformation("Auto-creating Source for Wikipedia Read: {Title} ({SourceId})", title, sourceId);
                     source = new Source
                     {
                         Id = technicalSourceId,
                         UserId = userId,
                         Type = effectiveType,
                         ExternalId = sourceId,
                         DisplayTitle = title,
                         IsTracked = false 
                     };
                     await _repository.CreateSourceAsync(source);
                 }
            }
            else if (effectiveType == SourceType.Document)
            {
                // For Documents, if Source not found by ID, it's an error because docs are local and must be known.
                throw new KeyNotFoundException($"Source entity not found for Document ID: {sourceId}");
            }
        }

        // 3. Ensure OnlineResource exists for Wikipedia/Web sources
        if (source.Type == SourceType.Wikipedia)
        {
            var existingResource = await _repository.GetOnlineResourceBySourceIdAsync(source.Id);
            if (existingResource == null)
            {
                var url = source.ExternalId;
                if (!url.StartsWith("http"))
                {
                    var lang = language ?? "en";
                    url = $"https://{lang}.wikipedia.org/wiki/{source.ExternalId}";
                }

                var onlineResource = new OnlineResource
                {
                    UserId = userId,
                    SourceId = source.Id,
                    URL = url,
                    Title = source.DisplayTitle,
                    Provider = "Wikipedia",
                    SavedAt = DateTime.UtcNow
                };
                await _repository.CreateOnlineResourceAsync(onlineResource);
            }
        }

        // 4. Select Content Source Handler
        var contentSource = _contentSources.FirstOrDefault(s => s.CanHandle(source.Type));
        if (contentSource == null) throw new InvalidOperationException($"No content source handler found for type {source.Type}");

        // 5. Fetch Content
        ContentResult content;
        try 
        {
            content = await contentSource.GetContentAsync(source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get content for source {SourceId}", source.Id);
            throw;
        }

        var dto = new CreateActivityDto
        {
            UserSessionId = sessionId,
            Title = content.Title,
            Description = activityType == ActivityType.Quiz ? "Quiz session" : (source.Type == SourceType.Wikipedia ? "Read from Wikipedia" : "Read from Document"),
            SourceId = source.Type == SourceType.Document ? source.Id : (!string.IsNullOrEmpty(source.ExternalId) ? source.ExternalId : source.Id),
            SourceType = source.Type,
            Type = activityType,
            SessionDateStart = DateTime.UtcNow,
            OriginExploreId = originExploreId,
            BacklogAddsCount = backlogAddsCount,
            RefreshCount = refreshCount,
            DurationSeconds = 0
        };

        if (!string.IsNullOrEmpty(originExploreId))
        {
            var exploreActivity = await _repository.GetByIdAsync(userId, originExploreId);
            if (exploreActivity != null)
            {
                dto.UserSessionId = exploreActivity.UserSessionId;
                
                // Mark exploration as completed
                exploreActivity.IsCompleted = true;
                exploreActivity.SessionDateEnd = DateTime.UtcNow;

                if (exploreDurationSeconds.HasValue) 
                {
                    exploreActivity.DurationSeconds = exploreDurationSeconds.Value;
                }
                await _repository.UpdateAsync(exploreActivity);
            }
        }

        var activity = await CreateActivityAsync(userId, dto);
        
        try 
        {
            source.TextContent = content.TextContent;
            await _repository.UpdateSourceAsync(source);
            _logger.LogInformation("Source {SourceId} updated with content. Content Length: {Length}", source.Id, source.TextContent?.Length ?? 0);
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "CRITICAL: Failed to persist content to database for Source {SourceId}.", source.Id);
        }

        activity.Source = source; // Assign after update to avoid EF tracking issues
        return activity;
    }

    public async Task<QuizDto> GenerateQuizAsync(string userId, string activityId)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException("Activity not found");

        if (activity.Type != ActivityType.Quiz)
        {
            _logger.LogWarning("Rejecting quiz generation for activity {ActivityId} of type {Type}. Only 'Quiz' activities are allowed.", activityId, activity.Type);
            throw new InvalidOperationException("Quiz generation is only permitted for activities of type 'Quiz'.");
        }

        var source = activity.Source ?? await _repository.GetSourceByIdAsync(activity.SourceId ?? string.Empty);
        if (source == null) throw new KeyNotFoundException("Source not found");

        string textToProcess = source.TextContent ?? string.Empty;
        if (string.IsNullOrEmpty(textToProcess)) throw new InvalidOperationException("No content available to generate quiz.");

        // Get user preferences for quiz generation
        var user = await _userRepository.GetByIdAsync(userId);
        var preferences = user?.Preferences ?? UserPreferences.Default(userId);
        
        var quizFormat = preferences.PreferredQuizFormat;
        var questionCount = preferences.QuestionsPerQuiz;
        var difficulty = preferences.DefaultDifficulty;
        var language = preferences.Language ?? "en";

        // Use a semaphore to ensure only one thread generates the quiz for this activity
        var activityLock = _quizLocks.GetOrAdd(activityId, _ => new SemaphoreSlim(1, 1));
        await activityLock.WaitAsync();

        try 
        {
            // Re-check after acquiring the lock to see if another thread already finished
            activity = await _repository.GetByIdAsync(userId, activityId);
            if (activity == null) throw new KeyNotFoundException("Activity not found.");

            if (!string.IsNullOrEmpty(activity.Payload))
            {
                try 
                {
                    var storedQuiz = _jsonSerializer.Deserialize<QuizDto>(activity.Payload);
                    if (storedQuiz != null && storedQuiz.Questions.Count > 0)
                    {
                        _logger.LogInformation("Returning previously generated quiz from activity payload (post-lock) for activity {ActivityId}", activityId);
                        return storedQuiz;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize stored quiz payload for activity {ActivityId}. Will regenerate.", activityId);
                }
            }

            var quiz = await _quizService.GenerateQuizAsync(textToProcess, quizFormat, questionCount, difficulty, language);
            
            // Store the quiz in the activity payload for later validation
            activity.Payload = _jsonSerializer.Serialize(quiz);
            await _repository.UpdateAsync(activity);
            
            return quiz;
        }
        finally 
        {
            activityLock.Release();
            // Optional: cleanup old locks if memory is a concern, but for now this is safer
        }
    }

    public async Task<QuizResultDto> SubmitQuizAsync(string userId, string activityId, QuizSubmissionDto submission)
    {
        var activity = await _repository.GetByIdAsync(userId, activityId);
        if (activity == null) throw new KeyNotFoundException("Activity not found");

        // Get user preferences for language
        var user = await _userRepository.GetByIdAsync(userId);
        var preferences = user?.Preferences ?? UserPreferences.Default(userId);
        var language = preferences.Language ?? "en";
        
        // Retrieve the stored quiz from the activity payload
        if (string.IsNullOrEmpty(activity.Payload))
        {
            throw new InvalidOperationException("Quiz data not found. Please regenerate the quiz.");
        }
        
        var quiz = _jsonSerializer.Deserialize<QuizDto>(activity.Payload);
        if (quiz == null || quiz.Questions.Count == 0)
        {
            throw new InvalidOperationException("Invalid quiz data. Please regenerate the quiz.");
        }
        
        var results = new List<QuestionResultDto>();
        int correctCount = 0;

        foreach (var answer in submission.Answers)
        {
            var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null)
            {
                _logger.LogWarning("Question {QuestionId} not found in generated quiz", answer.QuestionId);
                continue;
            }

            var result = new QuestionResultDto
            {
                QuestionId = answer.QuestionId,
                Explanation = question.Explanation
            };

            if (question.Type == "MCQ")
            {
                // MCQ evaluation: simple comparison
                var userAnswer = answer.SelectedOption ?? string.Empty;
                var correctAnswer = question.Options?[question.CorrectOptionIndex ?? 0] ?? string.Empty;
                
                result.IsCorrect = string.Equals(userAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
                result.UserAnswer = userAnswer;
                result.CorrectAnswer = correctAnswer;
                
                if (result.IsCorrect) correctCount++;
            }
            else if (question.Type == "OpenEnded")
            {
                // Open-ended evaluation: use LLM semantic comparison
                var userAnswer = answer.TextAnswer ?? string.Empty;
                var expectedAnswer = question.CorrectAnswer ?? string.Empty;
                
                var evaluation = await _quizService.EvaluateOpenAnswerAsync(question.Text, expectedAnswer, userAnswer, language);
                
                result.UserAnswer = userAnswer;
                result.CorrectAnswer = expectedAnswer;
                result.SemanticScore = evaluation.Score;
                result.Explanation = evaluation.Explanation;
                
                // Consider correct if semantic score >= 0.7
                result.IsCorrect = evaluation.Score >= 0.7;
                
                if (result.IsCorrect) correctCount++;
            }

            results.Add(result);
        }

        var totalQuestions = submission.Answers.Count;
        var scorePercentage = totalQuestions > 0 ? (double)correctCount / totalQuestions * 100.0 : 0.0;

        // Update the activity with quiz results
        var updateDto = new UpdateActivityDto
        {
            Score = correctCount,
            QuestionCount = totalQuestions,
            DurationSeconds = submission.DurationSeconds,
            SessionDateEnd = DateTime.UtcNow,
            IsCompleted = true
        };

        await UpdateActivityAsync(userId, activityId, updateDto);

        return new QuizResultDto
        {
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctCount,
            ScorePercentage = scorePercentage,
            Results = results
        };
    }

    public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
    {
        string? technicalSourceId = null;
        if (!string.IsNullOrEmpty(dto.SourceId))
        {
            // For Documents, the SourceId is already the Technical ID (GUID). Do not re-hash.
            if (dto.SourceType == SourceType.Document)
            {
                technicalSourceId = dto.SourceId;
            }
            else
            {
                 technicalSourceId = SourceHasher.GenerateId(dto.SourceType, dto.SourceId);
            }
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
        if (source == null && !string.IsNullOrEmpty(a.SourceId))
        {
             // Try to resolve if missing but ID is present (last resort, though repo should have loaded it)
             // _logger.LogWarning("Activity {Id} has SourceId {SourceId} but Source is null.", a.Id, a.SourceId);
        }

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
            IsCurrentBest = false, // Will be set by ApplyUniversalRules
            IsCompleted = a.IsCompleted,
            LlmModelName = a.LlmModelName,
            LlmVersion = a.LlmVersion,
            IsTracked = isTracked,
            ArticleContent = source?.TextContent,
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
