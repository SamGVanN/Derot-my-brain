using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

public class SqliteDocumentRepository : IDocumentRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteDocumentRepository> _logger;

    public SqliteDocumentRepository(DerotDbContext context, ILogger<SqliteDocumentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Document>> GetAllAsync(string userId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(string userId, string id)
    {
        return await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == id);
    }

    public async Task<Document?> GetBySourceIdAsync(string userId, string sourceId)
    {
        return await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.SourceId == sourceId);
    }

    public async Task<Document> CreateAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task DeleteAsync(string userId, string id)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == id);
            
        if (document != null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
