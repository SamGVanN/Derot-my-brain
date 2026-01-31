using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents a learning session (Read, Quiz) performed by a user on specific content.
/// Designed for progressive updates during the session lifecycle.
/// </summary>
public class UserActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    // --- Session Link ---

    /// <summary>
    /// The session this activity belongs to.
    /// </summary>
    public required string UserSessionId { get; set; }

    [JsonIgnore]
    public UserSession UserSession { get; set; } = null!;

    // --- Identification ---

    /// <summary>
    /// Foreign key to the Source entity.
    /// Acts as a direct link for easier querying, separate from session link.
    /// </summary>
    public string? SourceId { get; set; }

    [JsonIgnore]
    public Source? Source { get; set; }

    // --- Activity Metadata ---

    /// <summary>
    /// Snapshot of the title at the time of the activity.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Snapshot of the description or summary.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The nature of the session (Read, Quiz).
    /// </summary>
    public ActivityType Type { get; set; }

    // --- Timing & Session Data ---

    /// <summary>
    /// Exact date and time when the activity session started.
    /// </summary>
    public DateTime SessionDateStart { get; set; }

    /// <summary>
    /// Exact date and time when the activity session ended. 
    /// Nullable to allow insersion at session start.
    /// </summary>
    public DateTime? SessionDateEnd { get; set; }

    /// <summary>
    /// Duration of this specific activity in seconds.
    /// Replaces separate Exlore/Read/Quiz duration fields as logic is now atomic per activity.
    /// </summary>
    public int DurationSeconds { get; set; }


    // --- Metrics & Results ---

    /// <summary>
    /// Raw score achieved in the quiz.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Total number of questions presented in the quiz.
    /// Renamed from MaxScore for clarity.
    /// </summary>
    public int QuestionCount { get; set; }
    
    /// <summary>
    /// Normalized score (0-100). Null if the activity was only "Reading".
    /// </summary>
    public double? ScorePercentage { get; set; }
    
    /// <summary>
    /// True if this specific activity set a new "Best Score" record for this user and topic.
    /// </summary>
    public bool IsNewBestScore { get; set; }

    /// <summary>
    /// Indicates if this activity represents a baseline measurement for the user's knowledge on the topic.
    /// </summary>
    public bool IsBaseline { get; set; }

    /// <summary>
    /// Whether the session was completed (e.g., quiz finished).
    /// </summary>
    public bool IsCompleted { get; set; }
    
    // --- Detailed Data ---

    /// <summary>
    /// Cached article content used during this session.
    /// </summary>
    public string? ArticleContent { get; set; }

    /// <summary>
    /// Name of the LLM model used for generating/evaluating the quiz.
    /// </summary>
    public string? LlmModelName { get; set; } 

    /// <summary>
    /// Version of the LLM model used.
    /// </summary>
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// JSON blob containing the technical details of the session (e.g., questions, answers).
    /// </summary>
    public string? Payload { get; set; }

    // --- Explore-specific linkage & counters ---

    /// <summary>
    /// If this activity is an Explore session, and it later resulted in a Read activity,
    /// this field stores the Id of the corresponding Read `UserActivity`.
    /// Nullable when no Read followed the exploration.
    /// </summary>
    public string? ResultingReadActivityId { get; set; }

    /// <summary>
    /// Navigation property to the resulting Read activity (self-referencing).
    /// Nullable.
    /// </summary>
    [JsonIgnore]
    public UserActivity? ResultingReadActivity { get; set; }

    /// <summary>
    /// If this activity is a Read activity that originated from an Explore session,
    /// this field stores the Id of the corresponding Explore `UserActivity`.
    /// </summary>
    public string? OriginExploreId { get; set; }

    /// <summary>
    /// Number of articles the user added to their Backlog during this Explore session.
    /// Nullable: `null` means "not recorded", `0` means recorded and none were added.
    /// </summary>
    public int? BacklogAddsCount { get; set; }

    /// <summary>
    /// Number of discovery refreshes performed during this Explore session.
    /// </summary>
    public int RefreshCount { get; set; }
}
