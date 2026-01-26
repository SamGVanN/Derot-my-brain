using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

public class SqliteUserFocusRepository : IUserFocusRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteUserFocusRepository> _logger;
    
    public SqliteUserFocusRepository(DerotDbContext context, ILogger<SqliteUserFocusRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<UserFocus?> GetByIdAsync(string id)
    {
        return await _context.FocusAreas
            .Include(t => t.Source)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<UserFocus?> GetBySourceIdAsync(string userId, string sourceId)
    {
        return await _context.FocusAreas
            .Include(t => t.Source)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.SourceId == sourceId);
    }
    
    public async Task<IEnumerable<UserFocus>> GetAllAsync(string userId)
    {
        return await _context.FocusAreas
            .Include(t => t.Source)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.LastAttemptDate)
            .ToListAsync();
    }
    
    public async Task<UserFocus> CreateAsync(UserFocus userFocus)
    {
        _context.FocusAreas.Add(userFocus);
        await _context.SaveChangesAsync();
        return userFocus;
    }
    
    public async Task UpdateAsync(UserFocus userFocus)
    {
        _context.FocusAreas.Update(userFocus);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(string id)
    {
        var focus = await _context.FocusAreas.FindAsync(id);
        if (focus != null)
        {
            _context.FocusAreas.Remove(focus);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(string userId, string sourceId)
    {
        return await _context.FocusAreas.AnyAsync(t => t.UserId == userId && t.SourceId == sourceId);
    }
}
