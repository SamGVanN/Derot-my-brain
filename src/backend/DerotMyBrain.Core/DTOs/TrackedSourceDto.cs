using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class TrackedSourceDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty; // Technical GUID
    public string ExternalId { get; set; } = string.Empty; // URL or DocId
    public SourceType SourceType { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public string? Url { get; set; }

    // Stats
    public int BestScore { get; set; }
    public int LastScore { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public int TotalReadTimeSeconds { get; set; }
    public int TotalQuizTimeSeconds { get; set; }
    public int TotalStudyTimeSeconds { get; set; }

    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
    public bool IsInBacklog { get; set; }
}
