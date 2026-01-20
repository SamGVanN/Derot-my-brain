using DerotMyBrain.API.Data;
using DerotMyBrain.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DerotMyBrain.API.Repositories;

/// <summary>
/// SQLite implementation of the user repository.
/// </summary>
public class SqliteUserRepository : IUserRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteUserRepository> _logger;

    public SqliteUserRepository(DerotDbContext context, ILogger<SqliteUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByNameAsync(string name)
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Name.ToLower() == name.ToLower());
    }

    /// <inheritdoc/>
    public async Task<User> CreateAsync(User user)
    {
        _logger.LogInformation("Creating new user: {UserName}", user.Name);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    /// <inheritdoc/>
    public async Task<User> UpdateAsync(User user)
    {
        _logger.LogInformation("Updating user: {UserId}", user.Id);
        
        // Ensure the user exists and is tracked or attached
        var existingUser = await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {user.Id} not found.");
        }

        // Update properties
        existingUser.Name = user.Name;
        existingUser.LastConnectionAt = user.LastConnectionAt;
        // CreatedAt should ideally not change, but if needed: existingUser.CreatedAt = user.CreatedAt;

        // Update preferences
        if (existingUser.Preferences != null)
        {
            if (user.Preferences != null)
            {
                existingUser.Preferences.Language = user.Preferences.Language;
                existingUser.Preferences.PreferredTheme = user.Preferences.PreferredTheme;
                existingUser.Preferences.QuestionCount = user.Preferences.QuestionCount;
                existingUser.Preferences.SelectedCategories = user.Preferences.SelectedCategories;
            }
        }
        else if (user.Preferences != null)
        {
            // If existing had no preferences but new one does (should be rare given 1-to-1 required logic usually)
            user.Preferences.UserId = existingUser.Id;
            existingUser.Preferences = user.Preferences;
        }

        await _context.SaveChangesAsync();
        return existingUser;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string userId)
    {
        _logger.LogInformation("Deleting user: {UserId}", userId);
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
