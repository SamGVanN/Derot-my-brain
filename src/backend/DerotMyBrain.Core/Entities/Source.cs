using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Hub for content-related information. 
/// Every activity/session/document is linked to a Source.
/// </summary>
public class Source
{
    public string Id { get; set; } = string.Empty; // Deterministic Hash (or Guid for Docs)
    public string UserId { get; set; } = string.Empty;
    public SourceType Type { get; set; }
    public string ExternalId { get; set; } = string.Empty; // e.g., Wiki Page title, Document Guid
    public string DisplayTitle { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsTracked { get; set; } = false;
    public bool IsPinned { get; set; } = false;
    public bool IsArchived { get; set; } = false;

    [JsonIgnore]
    public User User { get; set; } = null!;

    public string? TopicId { get; set; }

    [JsonIgnore]
    public Topic? Topic { get; set; }

    [JsonIgnore]
    public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();

    [JsonIgnore]
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    [JsonIgnore]
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    [JsonIgnore]
    public ICollection<BacklogItem> BacklogItems { get; set; } = new List<BacklogItem>();
}
