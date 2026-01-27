using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// DTO for user activity session details.
/// </summary>
public class UserActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Session context
    public string UserSessionId { get; set; } = string.Empty;

    // Content Identity (still provided here for convenience, mapped from Source)
    public string SourceId { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string SourceHash { get; set; } = string.Empty;

    public ActivityType Type { get; set; }
    
    // Timing
    public DateTime SessionDateStart { get; set; }
    public DateTime? SessionDateEnd { get; set; }
    
    // Durations
    public int? ExploreDurationSeconds { get; set; }
    public int? ReadDurationSeconds { get; set; }
    public int? QuizDurationSeconds { get; set; }
    public int TotalDurationSeconds { get; set; }
    
    // Stats
    public int Score { get; set; }
    public int QuestionCount { get; set; }
    public double? ScorePercentage { get; set; }
    public bool IsNewBestScore { get; set; }
    public bool IsBaseline { get; set; }
    public bool IsCurrentBest { get; set; }
    public bool IsCompleted { get; set; }

    // LLM Info
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// Indicates if this topic is currently being tracked by the user.
    /// </summary>
    public bool IsTracked { get; set; }
    
    public string? ArticleContent { get; set; }
    public string? Payload { get; set; }
    
    // --- Explore-specific fields ---
    /// <summary>
    /// If this activity was an Explore session that later resulted in a Read,
    /// this holds the Id of the resulting Read UserActivity.
    /// </summary>
    public string? ResultingReadActivityId { get; set; }

    /// <summary>
    /// Number of items added to the Backlog during this Explore session.
    /// Nullable: null => not recorded, 0 => recorded and none were added.
    /// </summary>
    public int? BacklogAddsCount { get; set; }
}
