using System.Text.Json;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly string _seedDataDirectory;
    private readonly ILogger<CategoryService> _logger;
    private const string CategoriesFileName = "categories.json";

    public CategoryService(IConfiguration configuration, ILogger<CategoryService> logger)
    {
        var dataDirectory = configuration["DataDirectory"] ?? "Data";
        _seedDataDirectory = Path.Combine(dataDirectory, "seed");
        _logger = logger;
    }

    public async Task<IEnumerable<WikipediaCategory>> GetAllCategoriesAsync()
    {
        var filePath = Path.Combine(_seedDataDirectory, CategoriesFileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Categories file not found at {FilePath}", filePath);
            return new List<WikipediaCategory>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var categoryList = JsonSerializer.Deserialize<WikipediaCategoryList>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return categoryList?.Categories ?? new List<WikipediaCategory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading categories from {FilePath}", filePath);
            throw;
        }
    }
    
    private class WikipediaCategoryList
    {
        public List<WikipediaCategory> Categories { get; set; } = new();
    }
}
