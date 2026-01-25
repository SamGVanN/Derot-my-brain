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
    
    // Content Identity
    public string SourceId { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string SourceHash { get; set; } = string.Empty;

    public ActivityType Type { get; set; }
    
    // Timing
    public DateTime SessionDateStart { get; set; }
    public DateTime? SessionDateEnd { get; set; }
    
    // Durations
    public int? ReadDurationSeconds { get; set; }
    public int? QuizDurationSeconds { get; set; }
    public int TotalDurationSeconds { get; set; }
    
    // Stats
    public int Score { get; set; }
    public int QuestionCount { get; set; }
    public double? ScorePercentage { get; set; }
    public bool IsNewBestScore { get; set; }
    public bool IsCompleted { get; set; }

    // LLM Info
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// Indicates if this topic is currently being tracked by the user.
    /// </summary>
    public bool IsTracked { get; set; }
    
    public string? Payload { get; set; }
}
