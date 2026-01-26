using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class SourceDto
{
    public string Id { get; set; } = string.Empty;
    public SourceType Type { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DisplayTitle { get; set; } = string.Empty;
    public string? Url { get; set; }
}
