using System;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents a topic that the user has chosen to focus on.
/// Acts as a logical "Group By" for all UserActivity sharing the same SourceHash for a given user.
/// </summary>
public class UserFocus
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    // --- Identification ---

    /// <summary>
    /// Fixed-length technical key (SHA-256) used to link with UserActivity.
    /// </summary>
    public required string SourceHash { get; set; }

    /// <summary>
    /// The raw identifier (URL, Path) for display or re-fetching.
    /// </summary>
    public required string SourceId { get; set; }

    /// <summary>
    /// The origin of the content.
    /// </summary>
    public SourceType SourceType { get; set; }
    
    // --- User Customization ---

    /// <summary>
    /// User-defined title for this focus. Defaults to the source title.
    /// </summary>
    public string DisplayTitle { get; set; } = string.Empty;

    /// <summary>
    /// Whether the focus is pinned to the top of the list.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Whether the focus is archived (hidden from active view).
    /// </summary>
    public bool IsArchived { get; set; }

    // --- Aggregated Metrics (Rebuildable from UserActivity) ---

    /// <summary>
    /// The highest ScorePercentage ever achieved for this topic by this user.
    /// </summary>
    public double BestScore { get; set; }

    /// <summary>
    /// The ScorePercentage of the most recent activity.
    /// </summary>
    public double LastScore { get; set; }

    /// <summary>
    /// Date of the latest interaction (Read or Quiz) with this topic.
    /// </summary>
    public DateTime LastAttemptDate { get; set; }

    /// <summary>
    /// Total cumulative time spent reading this topic (in seconds).
    /// </summary>
    public int TotalReadTimeSeconds { get; set; }

    /// <summary>
    /// Total cumulative time spent on quizzes for this topic (in seconds).
    /// </summary>
    public int TotalQuizTimeSeconds { get; set; }

    /// <summary>
    /// Total cumulative study time (Read + Quiz) for this topic.
    /// </summary>
    public int TotalStudyTimeSeconds { get; set; }
}
