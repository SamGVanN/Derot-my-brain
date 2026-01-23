namespace DerotMyBrain.Core.DTOs;
    public class GeneralPreferencesDto
    {
        public string Language { get; set; } = string.Empty;
        public string PreferredTheme { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
    }

    public class CategoryPreferencesDto
    {
        public List<string> SelectedCategories { get; set; } = new List<string>();
    }

    public class DerotZonePreferencesDto
    {
        public int QuestionCount { get; set; }
        public List<string> SelectedCategories { get; set; } = new List<string>();
    }
