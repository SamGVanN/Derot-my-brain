namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// Data transfer object for top score entries.
/// </summary>
public class TopScoreDto
{
    public string ActivityId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Score { get; set; }
    
    /// <summary>
    /// Replaced TotalQuestions.
    /// </summary>
    public int QuestionCount { get; set; }
    
    public double Percentage { get; set; }
    public DateTime Date { get; set; }
}
