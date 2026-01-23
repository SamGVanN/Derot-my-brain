using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

public class UserActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    // Activity Details
    public required string Type { get; set; } // "Quiz", "Reading", etc.
    public required string Title { get; set; }
    public required string Description { get; set; }
    
    // Content Tracking
    public string? SourceUrl { get; set; }
    public string? ContentSourceType { get; set; } // "Wikipedia", "File", "Url"
    public string? ArticleContent { get; set; } // Cached content for the two-phase flow
    public string? LlmModelName { get; set; } // Model used for generation
    public string? LlmVersion { get; set; }
    
    // Quiz Specifics
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public double Percentage => MaxScore > 0 ? (double)Score / MaxScore * 100 : 0;
    
    public DateTime LastAttemptDate { get; set; }
    public bool IsCompleted { get; set; }
    
    // For "Daily Challenge" or specific tracking
    public bool IsTracked { get; set; }
    
    // JSON blob for storing questions/answers if needed for review
    public string? Payload { get; set; }
}
