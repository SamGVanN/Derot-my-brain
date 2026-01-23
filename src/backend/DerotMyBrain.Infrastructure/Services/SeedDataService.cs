using DerotMyBrain.Core.Entities;
using System.Text.Json;
using DerotMyBrain.Core.Interfaces.Services; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DerotMyBrain.Infrastructure.Services;

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
        _logger.LogInformation("Starting seed data initialization... (Stub)");
        await Task.CompletedTask;
    }

    public async Task<List<WikipediaCategory>> GetCategoriesAsync()
    {
        return await Task.FromResult(new List<WikipediaCategory>());
    }

    public async Task<List<Theme>> GetThemesAsync()
    {
        return await Task.FromResult(new List<Theme>());
    }
}
