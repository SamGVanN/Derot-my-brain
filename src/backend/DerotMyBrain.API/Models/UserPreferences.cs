using System.Text.Json.Serialization;

namespace DerotMyBrain.API.Models;

/// <summary>
/// User preferences for application settings.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// User ID (foreign key to User table).
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of questions to generate per quiz.
    /// </summary>
    public int QuestionCount { get; set; } = 10;
    
    /// <summary>
    /// Preferred UI theme name.
    /// </summary>
    public string PreferredTheme { get; set; } = "derot-brain";
    
    /// <summary>
    /// Preferred language: "en", "fr", or "auto".
    /// </summary>
    public string Language { get; set; } = "auto";
    
    /// <summary>
    /// List of selected Wikipedia category IDs.
    /// </summary>
    public List<string> SelectedCategories { get; set; } = new();
    
    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    [JsonIgnore]
    public User? User { get; set; }
}
