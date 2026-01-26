using System.Collections.Generic;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetAllAsync(string userId);
    Task<Document?> GetByIdAsync(string userId, string id);
    Task<Document?> GetBySourceIdAsync(string userId, string sourceId);
    Task<Document> CreateAsync(Document document);
    Task DeleteAsync(string userId, string id);
}
