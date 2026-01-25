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
        return await _context.UserFocuses.FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<UserFocus?> GetByHashAsync(string userId, string sourceHash)
    {
        return await _context.UserFocuses.FirstOrDefaultAsync(t => t.UserId == userId && t.SourceHash == sourceHash);
    }
    
    public async Task<IEnumerable<UserFocus>> GetAllAsync(string userId)
    {
        return await _context.UserFocuses
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.LastAttemptDate)
            .ToListAsync();
    }
    
    public async Task<UserFocus> CreateAsync(UserFocus userFocus)
    {
        _context.UserFocuses.Add(userFocus);
        await _context.SaveChangesAsync();
        return userFocus;
    }
    
    public async Task UpdateAsync(UserFocus userFocus)
    {
        _context.UserFocuses.Update(userFocus);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(string id)
    {
        var focus = await _context.UserFocuses.FindAsync(id);
        if (focus != null)
        {
            _context.UserFocuses.Remove(focus);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(string userId, string sourceHash)
    {
        return await _context.UserFocuses.AnyAsync(t => t.UserId == userId && t.SourceHash == sourceHash);
    }
}
