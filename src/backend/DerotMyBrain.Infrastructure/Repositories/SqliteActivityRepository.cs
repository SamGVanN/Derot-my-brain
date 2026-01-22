using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Repositories;

public class SqliteActivityRepository : IActivityRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteActivityRepository> _logger;
    
    public SqliteActivityRepository(DerotDbContext context, ILogger<SqliteActivityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<IEnumerable<UserActivity>> GetAllAsync(string userId)
    {
        return await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.LastAttemptDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic)
    {
        // Activity entity might store topic in Title or Description if not dedicated column
        // But in original code it had "Topic". In new Entity I removed "Topic" property in favor of generic properties?
        // Let me check Entity definition I wrote...
        // Ah, UserActivity.cs I wrote: Type, Title, Description, SourceUrl, ContentSourceType...
        // Does it have Topic? No.
        // But "Title" is likely the Topic for Wiki.
        // Wait, TrackedTopic uses "Name".
        // I need to ensure consistency.
        // Ideally "Title" is the topic name for Wiki.
        
        return await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Title == topic) // Assuming Title is Topic
            .OrderBy(a => a.LastAttemptDate)
            .ToListAsync();
    }
    
    public async Task<UserActivity?> GetByIdAsync(string userId, string activityId)
    {
        return await _context.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == activityId);
    }
    
    public async Task<UserActivity> CreateAsync(UserActivity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }
    
    public async Task<UserActivity> UpdateAsync(UserActivity activity)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync();
        return activity;
    }
    
    public async Task DeleteAsync(string userId, string activityId)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == activityId);
            
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<UserStatisticsDto> GetStatisticsAsync(string userId)
    {
        var activities = await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync();
        
        var stats = new UserStatisticsDto
        {
            TotalActivities = activities.Count,
            TotalQuizzes = activities.Count(a => a.Type == "Quiz" || a.Type == "Quiz_Pending"),
            TotalReads = activities.Count(a => a.Type == "Reading" || a.Type == "Read"),
            TrackedTopicsCount = await _context.TrackedTopics.CountAsync(t => t.UserId == userId)
        };

        var last = activities.OrderByDescending(a => a.LastAttemptDate).FirstOrDefault();
        if (last != null)
        {
            stats.LastActivity = new LastActivityDto
            {
                ActivityId = last.Id,
                Topic = last.Title,
                Date = last.LastAttemptDate,
                Type = last.Type
            };
        }
        
        // Best Score logic needs "Percentage" which is computed property, querying in memory is fine for low volume
        var best = activities
            .Where(a => a.MaxScore > 0)
            .OrderByDescending(a => a.Percentage)
            .FirstOrDefault();
            
        if (best != null)
        {
            stats.BestScore = new BestScoreDto
            {
                ActivityId = best.Id,
                Topic = best.Title,
                Score = best.Score,
                TotalQuestions = best.MaxScore,
                Percentage = best.Percentage,
                Date = best.LastAttemptDate
            };
        }
        
        return stats;
    }
    
    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
        var stats = await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.LastAttemptDate >= startDate)
            .GroupBy(a => a.LastAttemptDate.Date)
            .Select(g => new ActivityCalendarDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
            
        return stats;
    }
    
    public async Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit)
    {
        var scores = await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.MaxScore > 0)
            .OrderByDescending(a => (double)a.Score / a.MaxScore)
            .Take(limit)
            .ToListAsync();
            
        return scores.Select(a => new TopScoreDto
        {
            ActivityId = a.Id,
            Topic = a.Title,
            Score = a.Score,
            TotalQuestions = a.MaxScore,
            Percentage = a.Percentage,
            Date = a.LastAttemptDate
        });
    }
}
