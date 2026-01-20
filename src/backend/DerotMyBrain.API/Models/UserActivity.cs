namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a user's activity (reading or quiz) on a Wikipedia topic.
/// </summary>
public class UserActivity
{
    /// <summary>
    /// Unique identifier for the activity.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID of the user who performed this activity.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wikipedia topic/article title.
    /// </summary>
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Full Wikipedia URL for the article.
    /// </summary>
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time of the first attempt on this topic.
    /// </summary>
    public DateTime FirstAttemptDate { get; set; }
    
    /// <summary>
    /// Date and time of the most recent attempt on this topic.
    /// </summary>
    public DateTime LastAttemptDate { get; set; }
    
    /// <summary>
    /// Score from the most recent quiz attempt.
    /// </summary>
    public int LastScore { get; set; }
    
    /// <summary>
    /// Best score achieved across all attempts on this topic.
    /// </summary>
    public int BestScore { get; set; }
    
    /// <summary>
    /// Total number of questions in the quiz.
    /// </summary>
    public int TotalQuestions { get; set; }
    
    /// <summary>
    /// Name of the LLM model used to generate the quiz (e.g., "llama3:8b").
    /// </summary>
    public string? LlmModelName { get; set; }
    
    /// <summary>
    /// Version of the LLM model used.
    /// </summary>
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// Indicates whether this topic is tracked/favorited by the user.
    /// </summary>
    public bool IsTracked { get; set; }
    
    /// <summary>
    /// Type of activity: "Read" (article read) or "Quiz" (quiz completed).
    /// </summary>
    public string Type { get; set; } = "Quiz";
    
    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User? User { get; set; }
}
