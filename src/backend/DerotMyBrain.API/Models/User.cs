// Trigger reload
namespace DerotMyBrain.API.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime LastConnectionAt { get; set; }
        public UserPreferences Preferences { get; set; } = new UserPreferences();
    }

    public class UserPreferences
    {
        public int QuestionCount { get; set; } = 10;
        public string PreferredTheme { get; set; } = "derot-brain";
        public string Language { get; set; } = "auto"; // "en", "fr", or "auto"
        public List<string> SelectedCategories { get; set; } = new();
    }

    public class UserList
    {
        public List<User> Users { get; set; } = [];
    }
}
