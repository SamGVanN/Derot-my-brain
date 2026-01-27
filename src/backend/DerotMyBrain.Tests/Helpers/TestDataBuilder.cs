using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Utils;

namespace DerotMyBrain.Tests.Helpers;

/// <summary>
/// Fluent builder for creating User test entities with sensible defaults.
/// </summary>
public class UserBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _name = "Test User";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _lastConnectionAt = DateTime.UtcNow;
    private UserPreferences? _preferences;

    public UserBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public UserBuilder WithLastConnectionAt(DateTime lastConnectionAt)
    {
        _lastConnectionAt = lastConnectionAt;
        return this;
    }

    public UserBuilder WithPreferences(UserPreferences preferences)
    {
        _preferences = preferences;
        return this;
    }

    public User Build()
    {
        return new User
        {
            Id = _id,
            Name = _name,
            CreatedAt = _createdAt,
            LastConnectionAt = _lastConnectionAt,
            Preferences = _preferences ?? new PreferencesBuilder().WithUserId(_id).Build()
        };
    }
}

/// <summary>
/// Fluent builder for creating UserPreferences test entities with sensible defaults.
/// </summary>
public class PreferencesBuilder
{
    private string _userId = "test-user-id";
    private int _questionCount = 10;
    private string _preferredTheme = "derot-brain";
    private string _language = "en";
    private List<string> _selectedCategories = new();

    public PreferencesBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public PreferencesBuilder WithQuestionCount(int count)
    {
        _questionCount = count;
        return this;
    }

    public PreferencesBuilder WithTheme(string theme)
    {
        _preferredTheme = theme;
        return this;
    }

    public PreferencesBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    public PreferencesBuilder WithCategories(params string[] categories)
    {
        _selectedCategories = categories.ToList();
        return this;
    }

    public UserPreferences Build()
    {
        var categoryObjects = _selectedCategories.Select(c => new WikipediaCategory { Name = c }).ToList();

        return new UserPreferences
        {
            UserId = _userId,
            QuestionsPerQuiz = _questionCount,
            Theme = _preferredTheme,
            Language = _language,
            FavoriteCategories = categoryObjects
        };
    }
}

/// <summary>
/// Fluent builder for creating UserSession test entities.
/// </summary>
public class SessionBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _userId = "test-user-id";
    private string? _sourceId;
    private DateTime _startedAt = DateTime.UtcNow;
    private SessionStatus _status = SessionStatus.Active;

    public SessionBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public SessionBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public SessionBuilder WithSource(string sourceId)
    {
        _sourceId = sourceId;
        return this;
    }

    public SessionBuilder WithStatus(SessionStatus status)
    {
        _status = status;
        return this;
    }

    public UserSession Build()
    {
        return new UserSession
        {
            Id = _id,
            UserId = _userId,
            TargetSourceId = _sourceId,
            StartedAt = _startedAt,
            Status = _status
        };
    }
}

/// <summary>
/// Fluent builder for creating UserActivity test entities with sensible defaults.
/// </summary>
public class ActivityBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _userId = "test-user-id";
    private string _userSessionId = Guid.NewGuid().ToString();
    private string _title = "Test Topic";
    private ActivityType _type = ActivityType.Quiz;
    private DateTime _sessionDateStart = DateTime.UtcNow.AddMinutes(-30);
    private DateTime? _sessionDateEnd = DateTime.UtcNow;
    private int? _readDuration = 600;
    private int? _quizDuration = 900;
    private int _score = 8;
    private int _questionCount = 10;
    private string? _llmModelName;
    private string? _llmVersion;

    public ActivityBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ActivityBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public ActivityBuilder WithTopic(string topic)
    {
        _title = topic;
        return this;
    }

    public ActivityBuilder WithSession(string sessionId)
    {
        _userSessionId = sessionId;
        return this;
    }

    public ActivityBuilder WithType(ActivityType type)
    {
        _type = type;
        return this;
    }

    public ActivityBuilder AsQuiz(int score, int questionCount)
    {
        _type = ActivityType.Quiz;
        _score = score;
        _questionCount = questionCount;
        return this;
    }

    public ActivityBuilder AsRead()
    {
        _type = ActivityType.Read;
        _score = 0;
        _questionCount = 0;
        _quizDuration = 0;
        return this;
    }

    public ActivityBuilder WithTiming(DateTime start, DateTime? end, int? readSecs, int? quizSecs)
    {
        _sessionDateStart = start;
        _sessionDateEnd = end;
        _readDuration = readSecs;
        _quizDuration = quizSecs;
        return this;
    }

    public ActivityBuilder WithLlm(string model, string version)
    {
        _llmModelName = model;
        _llmVersion = version;
        return this;
    }

    public UserActivity Build()
    {
        double? percentage = null;
        if (_type == ActivityType.Quiz && _questionCount > 0)
        {
            percentage = (double)_score / _questionCount * 100.0;
        }

        // Simplification for test builder: assume duration is sum of passed values or just use one
        int duration = (_readDuration ?? 0) + (_quizDuration ?? 0);

        return new UserActivity
        {
            Id = _id,
            UserId = _userId,
            UserSessionId = _userSessionId,
            Title = _title,
            Description = $"{_type} session",
            Type = _type,
            SessionDateStart = _sessionDateStart,
            SessionDateEnd = _sessionDateEnd,
            DurationSeconds = duration,
            Score = _score,
            QuestionCount = _questionCount,
            ScorePercentage = percentage,
            IsCompleted = true,
            LlmModelName = _llmModelName,
            LlmVersion = _llmVersion
        };
    }
}
