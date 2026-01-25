using System.Collections.Generic;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Repositories;

public interface IBacklogRepository
{
    Task<IEnumerable<BacklogItem>> GetAllAsync(string userId);
    Task<BacklogItem?> GetBySourceHashAsync(string userId, string sourceHash);
    Task<BacklogItem> CreateAsync(BacklogItem item);
    Task DeleteAsync(string userId, string sourceHash);
}
