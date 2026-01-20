namespace DerotMyBrain.API.DTOs;

/// <summary>
/// Data transfer object for user statistics dashboard.
/// </summary>
public class UserStatisticsDto
{
    /// <summary>
    /// Total number of activities (Read + Quiz).
    /// </summary>
    public int TotalActivities { get; set; }
    
    /// <summary>
    /// Total number of quiz activities.
    /// </summary>
    public int TotalQuizzes { get; set; }
    
    /// <summary>
    /// Total number of read activities.
    /// </summary>
    public int TotalReads { get; set; }
    
    /// <summary>
    /// Number of tracked topics.
    /// </summary>
    public int TrackedTopicsCount { get; set; }
    
    /// <summary>
    /// Information about the last activity.
    /// </summary>
    public LastActivityDto? LastActivity { get; set; }
    
    /// <summary>
    /// Information about the best score achieved.
    /// </summary>
    public BestScoreDto? BestScore { get; set; }
}

/// <summary>
/// Data transfer object for last activity information.
/// </summary>
public class LastActivityDto
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
    /// Date of the activity.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Type of activity ("Read" or "Quiz").
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for best score information.
/// </summary>
public class BestScoreDto
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
