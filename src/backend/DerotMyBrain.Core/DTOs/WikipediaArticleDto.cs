namespace DerotMyBrain.Core.DTOs;

public class WikipediaArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Lang { get; set; }
    public string? SourceUrl { get; set; }
    public string? ImageUrl { get; set; }
}
