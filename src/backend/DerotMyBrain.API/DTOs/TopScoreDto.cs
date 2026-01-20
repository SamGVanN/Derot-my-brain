namespace DerotMyBrain.API.DTOs;

/// <summary>
/// Data transfer object for top score entries.
/// </summary>
public class TopScoreDto
{
    /// <summary>
    /// Activity identifier.
    /// </summary>
    public string ActivityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wikipedia topic/article title.
    /// </summary>
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Score achieved.
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// Total number of questions.
    /// </summary>
    public int TotalQuestions { get; set; }
    
    /// <summary>
    /// Percentage score (0-100).
    /// </summary>
    public double Percentage { get; set; }
    
    /// <summary>
    /// Date when the score was achieved.
    /// </summary>
    public DateTime Date { get; set; }
}
