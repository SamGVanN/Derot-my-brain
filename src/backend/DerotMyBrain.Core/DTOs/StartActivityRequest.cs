namespace DerotMyBrain.Core.DTOs;

public class StartActivityRequest
{
    /// <summary>
    /// Type of source: "RandomWiki", "Url", "File"
    /// </summary>
    public string Type { get; set; } = "RandomWiki";
    
    /// <summary>
    /// The source content (URL, or serialized filter for random)
    /// </summary>
    public string Filter { get; set; } = string.Empty;
}
