using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

public class SqliteUserRepository : IUserRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteUserRepository> _logger;

    public SqliteUserRepository(DerotDbContext context, ILogger<SqliteUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByNameAsync(string name)
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Name.ToLower() == name.ToLower());
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
