using System.ComponentModel.DataAnnotations;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// DTO for creating or initiating a new user activity session.
/// </summary>
public class CreateActivityDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// If provided, the activity will be linked to this session.
    /// If null, a new session will be created for the source.
    /// </summary>
    public string? UserSessionId { get; set; }
    
    [Required]
    public string SourceId { get; set; } = string.Empty;
    
    [Required]
    public SourceType SourceType { get; set; }

    [Required]
    public ActivityType Type { get; set; }

    // Timing
    [Required]
    public DateTime SessionDateStart { get; set; }
    
    /// <summary>
    /// Can be null if the activity is just being initiated.
    /// </summary>
    public DateTime? SessionDateEnd { get; set; }

    // Durations
    [Range(0, int.MaxValue)]
    public int? DurationSeconds { get; set; }

    // Quiz Metrics
    [Range(0, int.MaxValue)]
    public int? Score { get; set; }
    
    /// <summary>
    /// Replaces MaxScore.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? QuestionCount { get; set; }
    
    // LLM Metadata
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    public string? Payload { get; set; }
    
    // Optional: number of backlog additions performed during creation (useful for Explore creations)
    [Range(0, int.MaxValue)]
    public int? BacklogAddsCount { get; set; }

    /// <summary>
    /// Optional: when creating a Read that originates from a prior Explore session,
    /// provide the Explore activity Id so the service can link them transactionally.
    /// </summary>
    public string? OriginExploreId { get; set; }
}
