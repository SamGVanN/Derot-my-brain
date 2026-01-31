using System;
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
    
    public string SourceId { get; set; } = string.Empty; // GUID/Hash
    public string ExternalId { get; set; } = string.Empty; // URL or Doc GUID
    public SourceType SourceType { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
    
    // Session context
    public string UserSessionId { get; set; } = string.Empty;

    public ActivityType Type { get; set; }
    
    // Timing
    public DateTime SessionDateStart { get; set; }
    public DateTime? SessionDateEnd { get; set; }
    
    // Durations
    public int DurationSeconds { get; set; }
    
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
    /// Snapshot of the title of the Read article that originated from this exploration.
    /// Derived from navigation property in Service.
    /// </summary>
    public string? ResultingReadSourceName { get; set; }

    /// <summary>
    /// If this activity is a Read activity that originated from an Explore session,
    /// this holds the Id of that Explore UserActivity.
    /// </summary>
    public string? OriginExploreId { get; set; }

    /// <summary>
    /// Number of items added to the Backlog during this Explore session.
    /// Nullable: null => not recorded, 0 => recorded and none were added.
    /// </summary>
    public int? BacklogAddsCount { get; set; }

    /// <summary>
    /// Number of discovery refreshes performed during this Explore session.
    /// </summary>
    public int RefreshCount { get; set; }
}
