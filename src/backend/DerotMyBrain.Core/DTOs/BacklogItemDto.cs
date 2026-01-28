using System;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class BacklogItemDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string SourceHash => SourceId; // Alias for frontend compatibility
    public SourceType SourceType { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
