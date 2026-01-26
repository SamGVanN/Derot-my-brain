namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents a unique content source (Wikipedia article, Document, etc.).
/// Deters duplicates by using a deterministic ID based on Type and ExternalId.
/// </summary>
public class Source
{
    /// <summary>
    /// Deterministic ID (e.g., "Wikipedia:ArticleTitle" or composite hash).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    public SourceType Type { get; set; }

    /// <summary>
    /// The ID used by the provider (e.g., Page Title for Wikipedia).
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    public string DisplayTitle { get; set; } = string.Empty;

    public string? Url { get; set; }

    /// <summary>
    /// Navigation property to sessions associated with this source.
    /// </summary>
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    
    /// <summary>
    /// Navigation property to focus areas associated with this source.
    /// </summary>
    public ICollection<UserFocus> UserFocuses { get; set; } = new List<UserFocus>();
}
