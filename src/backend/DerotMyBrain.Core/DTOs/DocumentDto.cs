using System;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class DocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public ContentExtractionStatus? ContentExtractionStatus { get; set; }
    public string? ContentExtractionError { get; set; }
    public DateTime? ContentExtractionCompletedAt { get; set; }
}
