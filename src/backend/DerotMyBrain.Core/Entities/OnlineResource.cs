using System;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents an online resource saved by a user for learning activities.
/// </summary>
public class OnlineResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Title of the specific resource (e.g. original article title).
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider of the resource (e.g. "Wikipedia", "Youtube").
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// URL of the online resource.
    /// </summary>
    public required string URL { get; set; }
    
    /// <summary>
    /// Foreign key to the Source entity.
    /// </summary>
    public required string SourceId { get; set; }

    [JsonIgnore]
    public Source Source { get; set; } = null!;

    /// <summary>
    /// Date when the item was saved.
    /// </summary>
    public DateTime SavedAt { get; set; }
}
