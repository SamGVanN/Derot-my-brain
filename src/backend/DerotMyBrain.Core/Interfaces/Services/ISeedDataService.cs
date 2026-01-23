using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

/// <summary>
/// Interface for seed data service
/// </summary>
public interface ISeedDataService
{
    Task InitializeAsync();
    Task<List<WikipediaCategory>> GetCategoriesAsync();
    Task<List<Theme>> GetThemesAsync();
}
