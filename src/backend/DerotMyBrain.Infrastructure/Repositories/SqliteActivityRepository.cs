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
            .Include(a => a.UserSession)
                .ThenInclude(s => s.TargetSource)
            .Include(a => a.Source)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.SessionDateStart)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<UserActivity>> GetAllForContentAsync(string userId, string sourceId)
    {
        return await _context.Activities
            .AsNoTracking()
            .Include(a => a.UserSession)
            .Where(a => a.UserId == userId && a.UserSession.TargetSourceId == sourceId)
            .OrderBy(a => a.SessionDateStart)
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

    // Source Operations
    public async Task<Source?> GetSourceByIdAsync(string sourceId)
    {
        return await _context.Sources
            .Include(s => s.Activities)
            .FirstOrDefaultAsync(s => s.Id == sourceId);
    }

    public async Task<Source> CreateSourceAsync(Source source)
    {
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();
        return source;
    }

    public async Task<Source> UpdateSourceAsync(Source source)
    {
        _context.Sources.Update(source);
        await _context.SaveChangesAsync();
        return source;
    }

    public async Task<IEnumerable<Source>> GetTrackedSourcesAsync(string userId)
    {
        return await _context.Sources
            .AsNoTracking()
            .Include(s => s.Activities)
            .Where(s => s.UserId == userId && s.IsTracked)
            .ToListAsync();
    }

    // Session Operations
    public async Task<UserSession?> GetSessionByIdAsync(string userId, string sessionId)
    {
        return await _context.Sessions
            .Include(s => s.TargetSource)
            .Include(s => s.Activities)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);
    }

    public async Task<UserSession> CreateSessionAsync(UserSession session)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<UserSession> UpdateSessionAsync(UserSession session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<UserSession?> GetLastActiveSessionAsync(string userId, string sourceId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.TargetSourceId == sourceId && s.Status == SessionStatus.Active)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();
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
            TotalQuizzes = activities.Count(a => a.Type == ActivityType.Quiz),
            TotalReads = activities.Count(a => a.Type == ActivityType.Read),
            UserFocusCount = await _context.Sources.CountAsync(s => s.UserId == userId && s.IsTracked)
        };

        var last = activities.OrderByDescending(a => a.SessionDateStart).FirstOrDefault();
        if (last != null)
        {
            stats.LastActivity = new LastActivityDto
            {
                ActivityId = last.Id,
                Title = last.Title,
                Date = last.SessionDateEnd ?? last.SessionDateStart,
                Type = last.Type.ToString()
            };
        }
        
        var best = activities
            .Where(a => a.ScorePercentage.HasValue)
            .OrderByDescending(a => a.ScorePercentage)
            .FirstOrDefault();
            
        if (best != null)
        {
            stats.BestScore = new BestScoreDto
            {
                ActivityId = best.Id,
                Title = best.Title,
                Score = best.Score,
                QuestionCount = best.QuestionCount,
                Percentage = best.ScorePercentage ?? 0,
                Date = best.SessionDateEnd ?? best.SessionDateStart
            };
        }
        
        return stats;
    }
    
    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
        var stats = await _context.Activities
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.SessionDateStart >= startDate)
            .GroupBy(a => a.SessionDateStart.Date)
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
            .Where(a => a.UserId == userId && a.ScorePercentage.HasValue)
            .OrderByDescending(a => a.ScorePercentage)
            .Take(limit)
            .ToListAsync();
            
        return scores.Select(a => new TopScoreDto
        {
            ActivityId = a.Id,
            Title = a.Title,
            Score = a.Score,
            QuestionCount = a.QuestionCount,
            Percentage = a.ScorePercentage ?? 0,
            Date = a.SessionDateEnd ?? a.SessionDateStart
        });
    }
}
