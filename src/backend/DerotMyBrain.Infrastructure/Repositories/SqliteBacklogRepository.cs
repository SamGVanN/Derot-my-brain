using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

public class SqliteBacklogRepository : IBacklogRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteBacklogRepository> _logger;

    public SqliteBacklogRepository(DerotDbContext context, ILogger<SqliteBacklogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<BacklogItem>> GetAllAsync(string userId)
    {
        return await _context.BacklogItems
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.AddedAt)
            .ToListAsync();
    }

    public async Task<BacklogItem?> GetBySourceHashAsync(string userId, string sourceHash)
    {
        return await _context.BacklogItems
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.SourceHash == sourceHash);
    }

    public async Task<BacklogItem> CreateAsync(BacklogItem item)
    {
        _context.BacklogItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(string userId, string sourceHash)
    {
        var item = await _context.BacklogItems
            .FirstOrDefaultAsync(b => b.UserId == userId && b.SourceHash == sourceHash);
            
        if (item != null)
        {
            _context.BacklogItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
