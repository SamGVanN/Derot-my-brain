using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Services
{
    /// <summary>
    /// Interface for seed data service
    /// </summary>
    public interface ISeedDataService
    {
        Task InitializeAsync();
        Task<List<WikipediaCategory>> GetCategoriesAsync();
        Task<List<Theme>> GetThemesAsync();
    }
}
