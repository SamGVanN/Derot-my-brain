namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a topic that the user is tracking for mastery.
/// This is a denormalized cache table for performance.
/// All data can be rebuilt from UserActivity history.
/// </summary>
public class TrackedTopic
{
    /// <summary>
    /// Unique identifier for this tracked topic.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID of the user tracking this topic.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wikipedia topic/article title.
    /// This matches the Topic field in UserActivity.
    /// </summary>
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Full Wikipedia URL for the article.
    /// </summary>
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// When the user first marked this topic as tracked.
    /// Auto-set when tracking is enabled, cannot be changed by user.
    /// </summary>
    public DateTime TrackedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total number of Read sessions for this topic.
    /// Incremented from UserActivity where Type = "Read".
    /// </summary>
    public int TotalReadSessions { get; set; } = 0;
    
    /// <summary>
    /// Total number of Quiz attempts for this topic.
    /// Incremented from UserActivity where Type = "Quiz".
    /// </summary>
    public int TotalQuizAttempts { get; set; } = 0;
    
    /// <summary>
    /// Date of the first Read session for this topic.
    /// Null if user has never read this topic.
    /// </summary>
    public DateTime? FirstReadDate { get; set; }
    
    /// <summary>
    /// Date of the most recent Read session for this topic.
    /// Null if user has never read this topic.
    /// </summary>
    public DateTime? LastReadDate { get; set; }
    
    /// <summary>
    /// Date of the first Quiz attempt on this topic.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public DateTime? FirstAttemptDate { get; set; }
    
    /// <summary>
    /// Date of the most recent Quiz attempt on this topic.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public DateTime? LastAttemptDate { get; set; }
    
    /// <summary>
    /// Best (highest) score achieved across all quiz attempts on this topic.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public int? BestScore { get; set; }
    
    /// <summary>
    /// Total number of questions in the quiz that achieved the best score.
    /// Used to display "BestScore/TotalQuestions" (e.g., "9/10").
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public int? TotalQuestions { get; set; }
    
    /// <summary>
    /// Date when the best score was achieved.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public DateTime? BestScoreDate { get; set; }
    
    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User? User { get; set; }
}
