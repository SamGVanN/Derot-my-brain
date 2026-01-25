using System.ComponentModel.DataAnnotations;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class AddToBacklogDto
{
    [Required]
    public string SourceId { get; set; } = string.Empty;
    
    [Required]
    public SourceType SourceType { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
}
