using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

public enum SessionStatus
{
    Active = 0,
    Stopped = 1
}

/// <summary>
/// Represents a user's interaction session with a specific source.
/// A session can include multiple activities like Explore, Read, and Quiz.
/// </summary>
public class UserSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; } = null!;

    public string? TargetSourceId { get; set; }
    
    public Source? TargetSource { get; set; }

    public string? TargetTopicId { get; set; }

    public Topic? TargetTopic { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Active;

    /// <summary>
    /// Navigation property to activities performed during this session.
    /// </summary>
    public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();

    /// <summary>
    /// Total duration of all activities in this session in seconds.
    /// Calculated in-memory (not persisted to DB).
    /// </summary>
    public int TotalDurationSeconds => Activities?.Sum(a => a.DurationSeconds) ?? 0;
}
