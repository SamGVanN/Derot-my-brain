using DerotMyBrain.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.Core.DTOs;

public class TrackSourceRequestDto
{
    [Required]
    public string SourceId { get; set; } = string.Empty;
    
    [Required]
    public SourceType SourceType { get; set; }
    
    [Required]
    public string DisplayTitle { get; set; } = string.Empty;
}
