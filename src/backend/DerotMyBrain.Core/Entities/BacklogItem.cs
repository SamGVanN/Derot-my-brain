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
    /// Technical identifier for the source (FK to Source).
    /// </summary>
    public required string SourceId { get; set; }

    [JsonIgnore]
    public Source Source { get; set; } = null!;
    
    /// <summary>
    /// Title of the content for display in the backlog.
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Date when the item was added to the backlog.
    /// </summary>
    public DateTime AddedAt { get; set; }
}
