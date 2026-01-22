namespace DerotMyBrain.Core.Entities;

public class WikipediaCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string WikiIdentifier { get; set; } = string.Empty; // e.g., "Category:Technology"
    
    // UI / Localization properties
    public string NameFr { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WikipediaCategoryList
{
    public List<WikipediaCategory> Categories { get; set; } = new();
}
