using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

public class TrackedTopic
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public double MasteryLevel { get; set; } // 0-100%
    public DateTime LastInteraction { get; set; }
    public int InteractionCount { get; set; }
    
    /// <summary>
    /// JSON array of sub-concepts or related keywords found in this topic
    /// </summary>
    public string RelatedConceptsJson { get; set; } = "[]";
}
