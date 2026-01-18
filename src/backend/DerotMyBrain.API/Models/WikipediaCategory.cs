namespace DerotMyBrain.API.Models
{
    /// <summary>
    /// Represents a Wikipedia category for article filtering.
    /// These are the 13 official Wikipedia main categories.
    /// </summary>
    public class WikipediaCategory
    {
        /// <summary>
        /// Unique identifier for the category (e.g., "culture-arts")
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// English name of the category (e.g., "Culture and the arts")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// French name of the category (e.g., "Culture et arts")
        /// </summary>
        public string NameFr { get; set; } = string.Empty;

        /// <summary>
        /// Display order (1-13)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Whether this category is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Container for the list of Wikipedia categories
    /// </summary>
    public class WikipediaCategoryList
    {
        public List<WikipediaCategory> Categories { get; set; } = new();
    }
}
