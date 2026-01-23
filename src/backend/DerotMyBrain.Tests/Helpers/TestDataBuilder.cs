using DerotMyBrain.Core.Entities;

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
        return new UserPreferences
        {
            UserId = _userId,
            QuestionCount = _questionCount,
            PreferredTheme = _preferredTheme,
            Language = _language,
            SelectedCategories = _selectedCategories
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
    private string _topic = "Test Topic";
    private string _wikipediaUrl = "https://en.wikipedia.org/wiki/Test";
    private string _type = "Quiz";
    private DateTime _sessionDate = DateTime.UtcNow;
    private int? _score = 0;
    private int? _totalQuestions = 10;
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
        _topic = topic;
        return this;
    }

    public ActivityBuilder WithWikipediaUrl(string url)
    {
        _wikipediaUrl = url;
        return this;
    }

    public ActivityBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public ActivityBuilder AsQuiz(int score, int totalQuestions)
    {
        _type = "Quiz";
        _score = score;
        _totalQuestions = totalQuestions;
        return this;
    }

    public ActivityBuilder AsRead()
    {
        _type = "Read";
        _score = null;
        _totalQuestions = null;
        return this;
    }

    public ActivityBuilder WithScore(int? score)
    {
        _score = score;
        return this;
    }

    public ActivityBuilder WithSessionDate(DateTime date)
    {
        _sessionDate = date;
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
        return new UserActivity
        {
            Id = _id,
            UserId = _userId,
            Topic = _topic,
            WikipediaUrl = _wikipediaUrl,
            Type = _type,
            SessionDate = _sessionDate,
            Score = _score,
            TotalQuestions = _totalQuestions,
            LlmModelName = _llmModelName,
            LlmVersion = _llmVersion
        };
    }
}
