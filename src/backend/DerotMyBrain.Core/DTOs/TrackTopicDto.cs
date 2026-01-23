using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// DTO for tracking a topic.
/// </summary>
public class TrackTopicDto
{
    [Required]
    public string Topic { get; set; } = string.Empty;
    
    [Required]
    [Url]
    public string WikipediaUrl { get; set; } = string.Empty;
}
