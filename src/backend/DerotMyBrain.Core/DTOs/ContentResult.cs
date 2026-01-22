namespace DerotMyBrain.Core.DTOs;

public class ContentResult
{
    public string Title { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty; // Show this to user & send to LLM
    public string SourceUrl { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty; // "Wikipedia", "File", "Url"
}
