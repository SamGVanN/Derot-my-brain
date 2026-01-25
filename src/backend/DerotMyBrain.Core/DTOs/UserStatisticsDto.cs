namespace DerotMyBrain.Core.DTOs;

/// <summary>
/// Data transfer object for user statistics dashboard.
/// </summary>
public class UserStatisticsDto
{
    public int TotalActivities { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalReads { get; set; }
    public int UserFocusCount { get; set; }
    public LastActivityDto? LastActivity { get; set; }
    public BestScoreDto? BestScore { get; set; }
}

/// <summary>
/// Data transfer object for last activity information.
/// </summary>
public class LastActivityDto
{
    public string ActivityId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for best score information.
/// </summary>
public class BestScoreDto
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
