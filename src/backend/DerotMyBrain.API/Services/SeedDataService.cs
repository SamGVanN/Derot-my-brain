using DerotMyBrain.Core.Entities;
using System.Text.Json;
using DerotMyBrain.Infrastructure.Services; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.API.Services;

/// <summary>
/// Service responsible for initializing and managing seed data (categories and themes).
/// Seed data is immutable reference data deployed with the application.
/// </summary>
public class SeedDataService : ISeedDataService
{
    private readonly string _seedDataDirectory;
    private readonly ILogger<SeedDataService> _logger;
    private const string CategoriesFileName = "categories.json";
    private const string ThemesFileName = "themes.json";

    public SeedDataService(IConfiguration configuration, ILogger<SeedDataService> logger)
    {
        var dataDirectory = configuration["DataDirectory"] ?? "Data";
        _seedDataDirectory = Path.Combine(dataDirectory, "seed");
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting seed data initialization...");

        if (!Directory.Exists(_seedDataDirectory))
        {
            Directory.CreateDirectory(_seedDataDirectory);
            _logger.LogInformation("Created seed data directory: {Directory}", _seedDataDirectory);
        }

        await InitializeCategoriesAsync();
        await InitializeThemesAsync();

        _logger.LogInformation("Seed data initialization completed successfully");
    }

    public async Task<List<WikipediaCategory>> GetCategoriesAsync()
    {
        var filePath = Path.Combine(_seedDataDirectory, CategoriesFileName);
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Categories file not found. Initializing...");
            await InitializeCategoriesAsync();
        }

        try {
            var json = await File.ReadAllTextAsync(filePath);
            var categoryList = JsonSerializer.Deserialize<WikipediaCategoryListWrapper>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return categoryList?.Categories ?? new List<WikipediaCategory>();
        } catch(Exception ex) {
            _logger.LogError(ex, "Failed to read categories");
            return new List<WikipediaCategory>();
        }
    }

    public async Task<List<Theme>> GetThemesAsync()
    {
        var filePath = Path.Combine(_seedDataDirectory, ThemesFileName);
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Themes file not found. Initializing...");
            await InitializeThemesAsync();
        }

        try {
            var json = await File.ReadAllTextAsync(filePath);
            var themeList = JsonSerializer.Deserialize<ThemeListWrapper>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return themeList?.Themes ?? new List<Theme>();
        } catch(Exception ex) {
            _logger.LogError(ex, "Failed to read themes");
            return new List<Theme>();
        }
    }

    private async Task InitializeCategoriesAsync()
    {
        var filePath = Path.Combine(_seedDataDirectory, CategoriesFileName);

        if (File.Exists(filePath)) return;

        var categories = new WikipediaCategoryListWrapper
        {
            Categories = new List<WikipediaCategory>
            {
                new WikipediaCategory { Id = "general-reference", Name = "General reference", NameFr = "Référence générale", Order = 1, IsActive = true },
                new WikipediaCategory { Id = "culture-arts", Name = "Culture and the arts", NameFr = "Culture et arts", Order = 2, IsActive = true },
                new WikipediaCategory { Id = "geography-places", Name = "Geography and places", NameFr = "Géographie et lieux", Order = 3, IsActive = true },
                new WikipediaCategory { Id = "health-fitness", Name = "Health and fitness", NameFr = "Santé et forme", Order = 4, IsActive = true },
                new WikipediaCategory { Id = "history-events", Name = "History and events", NameFr = "Histoire et événements", Order = 5, IsActive = true },
                new WikipediaCategory { Id = "human-activities", Name = "Human activities", NameFr = "Activités humaines", Order = 6, IsActive = true },
                new WikipediaCategory { Id = "mathematics-logic", Name = "Mathematics and logic", NameFr = "Mathématiques et logique", Order = 7, IsActive = true },
                new WikipediaCategory { Id = "natural-sciences", Name = "Natural and physical sciences", NameFr = "Sciences naturelles et physiques", Order = 8, IsActive = true },
                new WikipediaCategory { Id = "people-self", Name = "People and self", NameFr = "Personnes et soi", Order = 9, IsActive = true },
                new WikipediaCategory { Id = "philosophy-thinking", Name = "Philosophy and thinking", NameFr = "Philosophie et pensée", Order = 10, IsActive = true },
                new WikipediaCategory { Id = "religion-belief", Name = "Religion and belief systems", NameFr = "Religion et systèmes de croyance", Order = 11, IsActive = true },
                new WikipediaCategory { Id = "society-sciences", Name = "Society and social sciences", NameFr = "Société et sciences sociales", Order = 12, IsActive = true },
                new WikipediaCategory { Id = "technology-sciences", Name = "Technology and applied sciences", NameFr = "Technologie et sciences appliquées", Order = 13, IsActive = true }
            }
        };

        var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation("Initialized 13 Wikipedia categories at {FilePath}", filePath);
    }

    private async Task InitializeThemesAsync()
    {
        var filePath = Path.Combine(_seedDataDirectory, ThemesFileName);

        if (File.Exists(filePath)) return;

        var themes = new ThemeListWrapper
        {
            Themes = new List<Theme>
            {
                new Theme { Id = "curiosity-loop", Name = "Curiosity Loop", Description = "Dark theme with blue accents", IsDefault = false, IsActive = true },
                new Theme { Id = "derot-brain", Name = "Derot Brain", Description = "Dark theme with violet accents", IsDefault = true, IsActive = true },
                new Theme { Id = "knowledge-core", Name = "Knowledge Core", Description = "Dark theme with cyan accents", IsDefault = false, IsActive = true },
                new Theme { Id = "mind-lab", Name = "Mind Lab", Description = "Dark theme with teal accents", IsDefault = false, IsActive = true },
                new Theme { Id = "neo-wikipedia", Name = "Neo-Wikipedia", Description = "Light theme with blue accents", IsDefault = false, IsActive = true }
            }
        };

        var json = JsonSerializer.Serialize(themes, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation("Initialized 5 UI themes at {FilePath}", filePath);
    }
}

// Internal wrappers to avoid conflict with Core entities if any
internal class WikipediaCategoryListWrapper
{
    public List<WikipediaCategory> Categories { get; set; } = new();
}

internal class ThemeListWrapper
{
    public List<Theme> Themes { get; set; } = new();
}
