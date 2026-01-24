using System;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

public class TrackedTopic
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public int BestScore { get; set; }
    public DateTime BestScoreDate { get; set; }
    public int TotalQuizAttempts { get; set; }
    public int TotalReadSessions { get; set; }
    
    public DateTime LastInteraction { get; set; }
}
