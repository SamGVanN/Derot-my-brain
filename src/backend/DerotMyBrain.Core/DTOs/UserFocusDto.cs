using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class UserFocusDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    // Identity
    public string SourceHash { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    
    // Stats
    public double BestScore { get; set; }
    public double LastScore { get; set; }
    public DateTime LastAttemptDate { get; set; }
    
    public int TotalReadTimeSeconds { get; set; }
    public int TotalQuizTimeSeconds { get; set; }
    public int TotalStudyTimeSeconds { get; set; }
    
    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
}
