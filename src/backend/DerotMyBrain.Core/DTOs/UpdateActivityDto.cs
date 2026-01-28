using System;
using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.Core.DTOs;

public class UpdateActivityDto
{
    [Range(0, int.MaxValue)]
    public int? Score { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? QuestionCount { get; set; }
    
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }

    public int? DurationSeconds { get; set; }
    public DateTime? SessionDateEnd { get; set; }
    public bool? IsCompleted { get; set; }

    // --- Explore linkage updates ---
    /// <summary>
    /// When an Explore transitions to a Read, the resulting Read activity Id can be set here.
    /// </summary>
    public string? ResultingReadActivityId { get; set; }

    /// <summary>
    /// Optional update to backlog additions count for Explore sessions.
    /// </summary>
    public int? BacklogAddsCount { get; set; }

    /// <summary>
    /// Optional update to refresh count for Explore sessions.
    /// </summary>
    public int? RefreshCount { get; set; }
}
