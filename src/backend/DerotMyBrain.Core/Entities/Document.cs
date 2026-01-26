using System;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents a document uploaded by a user for learning activities.
/// </summary>
public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Original name of the file when uploaded.
    /// </summary>
    public required string FileName { get; set; }
    
    /// <summary>
    /// Relative path to the stored file.
    /// </summary>
    public required string StoragePath { get; set; }
    
    /// <summary>
    /// Extension or mime type of the file.
    /// </summary>
    public required string FileType { get; set; }
    
    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Date and time when the document was uploaded.
    /// </summary>
    public DateTime UploadDate { get; set; }
    
    /// <summary>
    /// User-friendly title for the document.
    /// </summary>
    public string DisplayTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// Deterministic hash for linking with UserActivity and UserFocus.
    /// Generated from SourceType.Document and a unique identifier (e.g., relative StoragePath).
    /// </summary>
    public required string SourceHash { get; set; }
}
