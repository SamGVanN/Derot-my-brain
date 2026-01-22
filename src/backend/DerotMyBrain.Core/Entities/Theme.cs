namespace DerotMyBrain.Core.Entities;

/// <summary>
/// Represents a UI theme/color palette for the application.
/// </summary>
public class Theme
{
    /// <summary>
    /// Unique identifier for the theme (e.g., "derot-brain")
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the theme (e.g., "Derot Brain")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the theme (e.g., "Dark theme with violet accents")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default theme
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Whether this theme is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Container for the list of themes
/// </summary>
public class ThemeList
{
    public List<Theme> Themes { get; set; } = new();
}
