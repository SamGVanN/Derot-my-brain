using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Services
{
    public interface ICategoryService
    {
        Task<List<WikipediaCategory>> GetAllCategoriesAsync();
    }
}
