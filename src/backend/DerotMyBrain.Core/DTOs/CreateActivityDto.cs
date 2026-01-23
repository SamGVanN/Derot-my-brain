using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for creating a new user activity session.
/// </summary>
public class CreateActivityDto : IValidatableObject
{
    [Required]
    public string Topic { get; set; } = string.Empty;
    
    [Required]
    [Url]
    public string WikipediaUrl { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue)]
    public int? Score { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? TotalQuestions { get; set; }
    
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    [Required]
    [RegularExpression("^(Read|Quiz)$", ErrorMessage = "Type must be 'Read' or 'Quiz'")]
    public string Type { get; set; } = "Read";

    /// <summary>
    /// Custom validation: Quiz activities must have Score and TotalQuestions.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == "Quiz")
        {
            if (!Score.HasValue)
            {
                yield return new ValidationResult(
                    "Score is required for Quiz activities",
                    new[] { nameof(Score) });
            }

            if (!TotalQuestions.HasValue)
            {
                yield return new ValidationResult(
                    "TotalQuestions is required for Quiz activities",
                    new[] { nameof(TotalQuestions) });
            }
        }
    }
}
