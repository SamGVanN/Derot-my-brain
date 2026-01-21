namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for tracked topic summary with aggregated statistics.
/// </summary>
public class TrackedTopicDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// When the user first tracked this topic.
    /// </summary>
    public DateTime TrackedDate { get; set; }
    
    // Aggregated statistics
    public int TotalReadSessions { get; set; }
    public int TotalQuizAttempts { get; set; }
    
    public DateTime? FirstReadDate { get; set; }
    public DateTime? LastReadDate { get; set; }
    public DateTime? FirstAttemptDate { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    
    public int? BestScore { get; set; }
    public int? TotalQuestions { get; set; }
    public DateTime? BestScoreDate { get; set; }
}
