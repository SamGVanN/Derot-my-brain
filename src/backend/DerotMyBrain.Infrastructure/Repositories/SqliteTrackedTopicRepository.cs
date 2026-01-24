using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

public class SqliteTrackedTopicRepository : ITrackedTopicRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteTrackedTopicRepository> _logger;
    
    public SqliteTrackedTopicRepository(DerotDbContext context, ILogger<SqliteTrackedTopicRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<TrackedTopic?> GetByIdAsync(string id)
    {
        return await _context.TrackedTopics.FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<TrackedTopic?> GetByTitleAsync(string userId, string title)
    {
        return await _context.TrackedTopics.FirstOrDefaultAsync(t => t.UserId == userId && t.Title == title);
    }
    
    public async Task<IEnumerable<TrackedTopic>> GetAllAsync(string userId)
    {
        return await _context.TrackedTopics
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.LastInteraction)
            .ToListAsync();
    }
    
    public async Task<TrackedTopic> CreateAsync(TrackedTopic trackedTopic)
    {
        _context.TrackedTopics.Add(trackedTopic);
        await _context.SaveChangesAsync();
        return trackedTopic;
    }
    
    public async Task UpdateAsync(TrackedTopic trackedTopic)
    {
        _context.TrackedTopics.Update(trackedTopic);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(string id)
    {
        var topic = await _context.TrackedTopics.FindAsync(id);
        if (topic != null)
        {
            _context.TrackedTopics.Remove(topic);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(string userId, string title)
    {
        return await _context.TrackedTopics.AnyAsync(t => t.UserId == userId && t.Title == title);
    }
}
