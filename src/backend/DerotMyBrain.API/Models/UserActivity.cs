namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a single user session on a Wikipedia topic.
/// Each row is one discrete session (Read or Quiz).
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
    /// Type of this session: "Read" or "Quiz".
    /// Default is "Read" as reading happens before quizzing.
    /// </summary>
    public string Type { get; set; } = "Read";
    
    /// <summary>
    /// When this session occurred.
    /// </summary>
    public DateTime SessionDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Score from this quiz session.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public int? Score { get; set; }
    
    /// <summary>
    /// Total number of questions in this quiz session.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public int? TotalQuestions { get; set; }
    
    /// <summary>
    /// Name of the LLM model used to generate the quiz (e.g., "llama3:8b").
    /// </summary>
    public string? LlmModelName { get; set; }
    
    /// <summary>
    /// Version of the LLM model used.
    /// </summary>
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User? User { get; set; }
}

