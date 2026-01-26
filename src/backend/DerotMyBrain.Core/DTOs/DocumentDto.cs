using System;

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
    public string SourceHash { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
}
