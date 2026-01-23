using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.Core.DTOs;

public class UpdateActivityDto
{
    [Range(0, int.MaxValue)]
    public int? Score { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? TotalQuestions { get; set; }
    
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
}
