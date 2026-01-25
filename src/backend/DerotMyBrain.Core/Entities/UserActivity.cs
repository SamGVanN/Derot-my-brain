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
    
    // --- Content Identification ---

    /// <summary>
    /// The technical identifier for the source (e.g., full Wikipedia URL or absolute file path).
    /// </summary>
    public required string SourceId { get; set; }

    /// <summary>
    /// The origin of the content.
    /// </summary>
    public SourceType SourceType { get; set; }

    /// <summary>
    /// A deterministic SHA-256 hash of (SourceType + SourceId).
    /// Used as a fixed-length key for efficient indexing and logical relationships.
    /// </summary>
    public required string SourceHash { get; set; }

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
    /// Time spent actively exploring the content (in seconds).
    /// Nullable if the session hasn't finished the 'Explore' phase.
    /// </summary>
    public int? ExploreDurationSeconds { get; set; }

    /// <summary>
    /// Time spent actively reading or exploring the content (in seconds).
    /// Nullable if the session hasn't finished the 'Read' phase.
    /// </summary>
    public int? ReadDurationSeconds { get; set; }

    /// <summary>
    /// Time spent actively answering the quiz (in seconds).
    /// Nullable if the session is Read-only or Quiz hasn't started.
    /// </summary>
    public int? QuizDurationSeconds { get; set; }

    /// <summary>
    /// Total active study time (Read + Quiz) in seconds.
    /// Calculated dynamically.
    /// </summary>
    public int TotalDurationSeconds => (ReadDurationSeconds ?? 0) + (QuizDurationSeconds ?? 0);

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
}
