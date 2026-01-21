using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Repositories;

/// <summary>
/// SQLite implementation of ITrackedTopicRepository.
/// </summary>
public class SqliteTrackedTopicRepository : ITrackedTopicRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteTrackedTopicRepository> _logger;
    
    public SqliteTrackedTopicRepository(
        DerotDbContext context, 
        ILogger<SqliteTrackedTopicRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<TrackedTopic?> GetByIdAsync(string id)
    {
        try
        {
            return await _context.TrackedTopics
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tracked topic {TrackedTopicId}", id);
            throw;
        }
    }
    
    public async Task<TrackedTopic?> GetByTopicAsync(string userId, string topic)
    {
        try
        {
            return await _context.TrackedTopics
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Topic == topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tracked topic for user {UserId}, topic {Topic}", userId, topic);
            throw;
        }
    }
    
    public async Task<IEnumerable<TrackedTopic>> GetAllAsync(string userId)
    {
        try
        {
            return await _context.TrackedTopics
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.LastAttemptDate ?? t.LastReadDate ?? t.TrackedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tracked topics for user {UserId}", userId);
            throw;
        }
    }
    
    public async Task<TrackedTopic> CreateAsync(TrackedTopic trackedTopic)
    {
        try
        {
            _logger.LogInformation("Creating tracked topic for user {UserId}, topic {Topic}", 
                trackedTopic.UserId, trackedTopic.Topic);
            
            _context.TrackedTopics.Add(trackedTopic);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Tracked topic created: {TrackedTopicId}", trackedTopic.Id);
            return trackedTopic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tracked topic for user {UserId}, topic {Topic}", 
                trackedTopic.UserId, trackedTopic.Topic);
            throw;
        }
    }
    
    public async Task UpdateAsync(TrackedTopic trackedTopic)
    {
        try
        {
            _context.TrackedTopics.Update(trackedTopic);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Tracked topic updated: {TrackedTopicId}", trackedTopic.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tracked topic {TrackedTopicId}", trackedTopic.Id);
            throw;
        }
    }
    
    public async Task DeleteAsync(string id)
    {
        try
        {
            var trackedTopic = await GetByIdAsync(id);
            if (trackedTopic != null)
            {
                _context.TrackedTopics.Remove(trackedTopic);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Tracked topic deleted: {TrackedTopicId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete tracked topic {TrackedTopicId}", id);
            throw;
        }
    }
    
    public async Task<bool> ExistsAsync(string userId, string topic)
    {
        try
        {
            return await _context.TrackedTopics
                .AnyAsync(t => t.UserId == userId && t.Topic == topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if topic exists for user {UserId}, topic {Topic}", userId, topic);
            throw;
        }
    }
}
