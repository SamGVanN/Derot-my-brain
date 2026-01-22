using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DerotMyBrain.Core.Entities;

public class UserPreferences
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "system";
    
    public List<WikipediaCategory> FavoriteCategories { get; set; } = new();
    
    // Quiz Preferences
    public int QuestionsPerQuiz { get; set; } = 5;
    public string DefaultDifficulty { get; set; } = "Medium";
    
    public static UserPreferences Default() => new()
    {
        Language = "en",
        Theme = "system",
        QuestionsPerQuiz = 5,
        DefaultDifficulty = "Medium"
    };
}
