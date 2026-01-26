using System;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents an item in the user's backlog of content to be processed later.
/// </summary>
public class BacklogItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Technical identifier for the source (Wikipedia URL or File ID).
    /// </summary>
    public required string SourceId { get; set; }
    
    /// <summary>
    /// Origin of the content.
    /// </summary>
    public SourceType SourceType { get; set; }
    
    /// <summary>
    /// Deterministic hash of (SourceType + SourceId).
    /// Used for duplicate prevention and linking.
    /// </summary>
    public required string SourceHash { get; set; }
    
    /// <summary>
    /// Title of the content for display in the backlog.
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Date when the item was added to the backlog.
    /// </summary>
    public DateTime AddedAt { get; set; }
}
