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
    public int? ReadDurationSeconds { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? QuizDurationSeconds { get; set; }

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
}
