using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<WikipediaCategory>> GetAllCategoriesAsync();
}
