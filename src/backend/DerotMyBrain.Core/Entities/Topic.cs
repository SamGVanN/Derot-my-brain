using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents an organizational folder or themed area that groups multiple Sources.
/// Allows scaling learning activities from single sources to broader topics.
/// </summary>
public class Topic
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    public required string Title { get; set; }
    
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to sources grouped under this topic.
    /// </summary>
    public ICollection<Source> Sources { get; set; } = new List<Source>();
}
