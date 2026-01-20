using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.DTOs;

namespace DerotMyBrain.API.Repositories;

/// <summary>
/// SQLite implementation of the activity repository using Entity Framework Core.
/// </summary>
public class SqliteActivityRepository : IActivityRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteActivityRepository> _logger;
    
    /// <summary>
    /// Initializes a new instance of the SqliteActivityRepository class.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public SqliteActivityRepository(DerotDbContext context, ILogger<SqliteActivityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    #region CRUD Operations
    
    /// <inheritdoc/>
    public async Task<IEnumerable<UserActivity>> GetAllAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting all activities for user {UserId}", userId);
            
            return await _context.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.LastAttemptDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all activities for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<UserActivity>> GetTrackedAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting tracked activities for user {UserId}", userId);
            
            return await _context.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsTracked)
                .OrderByDescending(a => a.LastAttemptDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracked activities for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserActivity?> GetByIdAsync(string userId, string activityId)
    {
        try
        {
            _logger.LogInformation("Getting activity {ActivityId} for user {UserId}", activityId, userId);
            
            return await _context.Activities
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == activityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity {ActivityId} for user {UserId}", activityId, userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserActivity> CreateAsync(UserActivity activity)
    {
        try
        {
            _logger.LogInformation("Creating activity for user {UserId}, topic {Topic}", activity.UserId, activity.Topic);
            
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Activity {ActivityId} created successfully", activity.Id);
            return activity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity for user {UserId}", activity.UserId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserActivity> UpdateAsync(UserActivity activity)
    {
        try
        {
            _logger.LogInformation("Updating activity {ActivityId} for user {UserId}", activity.Id, activity.UserId);
            
            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Activity {ActivityId} updated successfully", activity.Id);
            return activity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {ActivityId}", activity.Id);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task DeleteAsync(string userId, string activityId)
    {
        try
        {
            _logger.LogInformation("Deleting activity {ActivityId} for user {UserId}", activityId, userId);
            
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == activityId);
            
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Activity {ActivityId} deleted successfully", activityId);
            }
            else
            {
                _logger.LogWarning("Activity {ActivityId} not found for deletion", activityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {ActivityId}", activityId);
            throw;
        }
    }
    
    #endregion
    
    #region Dashboard Queries
    
    /// <inheritdoc/>
    public async Task<UserStatisticsDto> GetStatisticsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting statistics for user {UserId}", userId);
            
            var activities = await _context.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync();
            
            var statistics = new UserStatisticsDto
            {
                TotalActivities = activities.Count,
                TotalQuizzes = activities.Count(a => a.Type == "Quiz"),
                TotalReads = activities.Count(a => a.Type == "Read"),
                TrackedTopicsCount = activities.Count(a => a.IsTracked)
            };
            
            // Get last activity
            var lastActivity = activities
                .OrderByDescending(a => a.LastAttemptDate)
                .FirstOrDefault();
            
            if (lastActivity != null)
            {
                statistics.LastActivity = new LastActivityDto
                {
                    ActivityId = lastActivity.Id,
                    Topic = lastActivity.Topic,
                    Date = lastActivity.LastAttemptDate,
                    Type = lastActivity.Type
                };
            }
            
            // Get best score (highest percentage)
            var bestScoreActivity = activities
                .Where(a => a.TotalQuestions > 0)
                .OrderByDescending(a => (double)a.BestScore / a.TotalQuestions)
                .FirstOrDefault();
            
            if (bestScoreActivity != null)
            {
                statistics.BestScore = new BestScoreDto
                {
                    ActivityId = bestScoreActivity.Id,
                    Topic = bestScoreActivity.Topic,
                    Score = bestScoreActivity.BestScore,
                    TotalQuestions = bestScoreActivity.TotalQuestions,
                    Percentage = Math.Round((double)bestScoreActivity.BestScore / bestScoreActivity.TotalQuestions * 100, 1),
                    Date = bestScoreActivity.LastAttemptDate
                };
            }
            
            _logger.LogInformation("Statistics retrieved successfully for user {UserId}", userId);
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ActivityCalendarDto>> GetActivityCalendarAsync(string userId, int days)
    {
        try
        {
            _logger.LogInformation("Getting activity calendar for user {UserId}, last {Days} days", userId, days);
            
            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
            
            var calendar = await _context.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.LastAttemptDate >= startDate)
                .GroupBy(a => a.LastAttemptDate.Date)
                .Select(g => new ActivityCalendarDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(c => c.Date)
                .ToListAsync();
            
            _logger.LogInformation("Activity calendar retrieved successfully for user {UserId}", userId);
            return calendar;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity calendar for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<TopScoreDto>> GetTopScoresAsync(string userId, int limit)
    {
        try
        {
            _logger.LogInformation("Getting top {Limit} scores for user {UserId}", limit, userId);
            
            var topScores = await _context.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.TotalQuestions > 0)
                .OrderByDescending(a => (double)a.BestScore / a.TotalQuestions)
                .ThenByDescending(a => a.LastAttemptDate)
                .Take(limit)
                .Select(a => new TopScoreDto
                {
                    ActivityId = a.Id,
                    Topic = a.Topic,
                    Score = a.BestScore,
                    TotalQuestions = a.TotalQuestions,
                    Percentage = Math.Round((double)a.BestScore / a.TotalQuestions * 100, 1),
                    Date = a.LastAttemptDate
                })
                .ToListAsync();
            
            _logger.LogInformation("Top scores retrieved successfully for user {UserId}", userId);
            return topScores;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top scores for user {UserId}", userId);
            throw;
        }
    }
    
    #endregion
}
