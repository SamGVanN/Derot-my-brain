using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using DerotMyBrain.API.DTOs;

namespace DerotMyBrain.Tests.Repositories;

/// <summary>
/// Unit tests for SqliteActivityRepository following TDD methodology.
/// </summary>
public class SqliteActivityRepositoryTests : IDisposable
{
    private readonly DerotDbContext _context;
    private readonly SqliteActivityRepository _repository;
    private readonly Mock<ILogger<SqliteActivityRepository>> _mockLogger;
    
    public SqliteActivityRepositoryTests()
    {
        // Create InMemory database with unique name for test isolation
        var options = new DbContextOptionsBuilder<DerotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new DerotDbContext(options);
        _mockLogger = new Mock<ILogger<SqliteActivityRepository>>();
        _repository = new SqliteActivityRepository(_context, _mockLogger.Object);
    }
    
    #region CRUD Tests
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActivities_ForUser()
    {
        // Arrange
        var userId = "test-user-001";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activities = new List<UserActivity>
        {
            new UserActivity 
            { 
                UserId = userId, 
                Topic = "Topic 1", 
                WikipediaUrl = "https://en.wikipedia.org/wiki/Topic_1", 
                FirstAttemptDate = DateTime.UtcNow.AddDays(-2), 
                LastAttemptDate = DateTime.UtcNow.AddDays(-2), 
                LastScore = 10, 
                BestScore = 10, 
                TotalQuestions = 10, 
                Type = "Quiz" 
            },
            new UserActivity 
            { 
                UserId = userId, 
                Topic = "Topic 2", 
                WikipediaUrl = "https://en.wikipedia.org/wiki/Topic_2", 
                FirstAttemptDate = DateTime.UtcNow.AddDays(-1), 
                LastAttemptDate = DateTime.UtcNow.AddDays(-1), 
                LastScore = 15, 
                BestScore = 15, 
                TotalQuestions = 20, 
                Type = "Quiz" 
            }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetAllAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(userId, a.UserId));
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoActivities()
    {
        // Arrange
        var userId = "test-user-002";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 2", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetAllAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task GetTrackedAsync_ShouldReturnOnlyTrackedActivities()
    {
        // Arrange
        var userId = "test-user-003";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 3", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activities = new List<UserActivity>
        {
            new UserActivity 
            { 
                UserId = userId, 
                Topic = "Tracked Topic", 
                WikipediaUrl = "https://en.wikipedia.org/wiki/Tracked", 
                FirstAttemptDate = DateTime.UtcNow, 
                LastAttemptDate = DateTime.UtcNow, 
                LastScore = 10, 
                BestScore = 10, 
                TotalQuestions = 10, 
                Type = "Quiz",
                IsTracked = true
            },
            new UserActivity 
            { 
                UserId = userId, 
                Topic = "Non-Tracked Topic", 
                WikipediaUrl = "https://en.wikipedia.org/wiki/NonTracked", 
                FirstAttemptDate = DateTime.UtcNow, 
                LastAttemptDate = DateTime.UtcNow, 
                LastScore = 15, 
                BestScore = 15, 
                TotalQuestions = 20, 
                Type = "Quiz",
                IsTracked = false
            }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetTrackedAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, a => Assert.True(a.IsTracked));
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnActivity_WhenExists()
    {
        // Arrange
        var userId = "test-user-004";
        var activityId = Guid.NewGuid().ToString();
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 4", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activity = new UserActivity 
        { 
            Id = activityId,
            UserId = userId, 
            Topic = "Specific Topic", 
            WikipediaUrl = "https://en.wikipedia.org/wiki/Specific", 
            FirstAttemptDate = DateTime.UtcNow, 
            LastAttemptDate = DateTime.UtcNow, 
            LastScore = 10, 
            BestScore = 10, 
            TotalQuestions = 10, 
            Type = "Quiz" 
        };
        
        _context.Users.Add(user);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByIdAsync(userId, activityId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(activityId, result.Id);
        Assert.Equal("Specific Topic", result.Topic);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var userId = "test-user-005";
        var invalidActivityId = Guid.NewGuid().ToString();
        
        // Act
        var result = await _repository.GetByIdAsync(userId, invalidActivityId);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldAddActivity_ToDatabase()
    {
        // Arrange
        var userId = "test-user-006";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 6", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var activity = new UserActivity 
        { 
            UserId = userId, 
            Topic = "New Topic", 
            WikipediaUrl = "https://en.wikipedia.org/wiki/New", 
            FirstAttemptDate = DateTime.UtcNow, 
            LastAttemptDate = DateTime.UtcNow, 
            LastScore = 8, 
            BestScore = 8, 
            TotalQuestions = 10, 
            Type = "Quiz" 
        };
        
        // Act
        var result = await _repository.CreateAsync(activity);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        
        var savedActivity = await _context.Activities.FindAsync(result.Id);
        Assert.NotNull(savedActivity);
        Assert.Equal("New Topic", savedActivity.Topic);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldModifyActivity_InDatabase()
    {
        // Arrange
        var userId = "test-user-007";
        var activityId = Guid.NewGuid().ToString();
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 7", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activity = new UserActivity 
        { 
            Id = activityId,
            UserId = userId, 
            Topic = "Original Topic", 
            WikipediaUrl = "https://en.wikipedia.org/wiki/Original", 
            FirstAttemptDate = DateTime.UtcNow, 
            LastAttemptDate = DateTime.UtcNow, 
            LastScore = 5, 
            BestScore = 5, 
            TotalQuestions = 10, 
            Type = "Quiz" 
        };
        
        _context.Users.Add(user);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        
        // Detach to simulate update scenario
        _context.Entry(activity).State = EntityState.Detached;
        
        // Modify activity
        activity.LastScore = 10;
        activity.BestScore = 10;
        activity.LastAttemptDate = DateTime.UtcNow.AddHours(1);
        
        // Act
        var result = await _repository.UpdateAsync(activity);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.LastScore);
        Assert.Equal(10, result.BestScore);
        
        var updatedActivity = await _context.Activities.FindAsync(activityId);
        Assert.NotNull(updatedActivity);
        Assert.Equal(10, updatedActivity.LastScore);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldRemoveActivity_FromDatabase()
    {
        // Arrange
        var userId = "test-user-008";
        var activityId = Guid.NewGuid().ToString();
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 8", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activity = new UserActivity 
        { 
            Id = activityId,
            UserId = userId, 
            Topic = "To Delete", 
            WikipediaUrl = "https://en.wikipedia.org/wiki/Delete", 
            FirstAttemptDate = DateTime.UtcNow, 
            LastAttemptDate = DateTime.UtcNow, 
            LastScore = 10, 
            BestScore = 10, 
            TotalQuestions = 10, 
            Type = "Quiz" 
        };
        
        _context.Users.Add(user);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        
        // Act
        await _repository.DeleteAsync(userId, activityId);
        
        // Assert
        var deletedActivity = await _context.Activities.FindAsync(activityId);
        Assert.Null(deletedActivity);
    }
    
    #endregion
    
    #region Dashboard Tests
    
    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var userId = "test-user-009";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 9", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activities = new List<UserActivity>
        {
            new UserActivity { UserId = userId, Topic = "Topic 1", WikipediaUrl = "url1", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 10, BestScore = 10, TotalQuestions = 10, Type = "Quiz", IsTracked = true },
            new UserActivity { UserId = userId, Topic = "Topic 2", WikipediaUrl = "url2", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 15, BestScore = 15, TotalQuestions = 20, Type = "Quiz", IsTracked = false },
            new UserActivity { UserId = userId, Topic = "Topic 3", WikipediaUrl = "url3", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 0, BestScore = 0, TotalQuestions = 10, Type = "Read", IsTracked = true }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetStatisticsAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalActivities);
        Assert.Equal(2, result.TotalQuizzes);
        Assert.Equal(1, result.TotalReads);
        Assert.Equal(2, result.TrackedTopicsCount);
    }
    
    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnLastActivity()
    {
        // Arrange
        var userId = "test-user-010";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 10", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activities = new List<UserActivity>
        {
            new UserActivity { UserId = userId, Topic = "Old Topic", WikipediaUrl = "url1", FirstAttemptDate = DateTime.UtcNow.AddDays(-5), LastAttemptDate = DateTime.UtcNow.AddDays(-5), LastScore = 10, BestScore = 10, TotalQuestions = 10, Type = "Quiz" },
            new UserActivity { UserId = userId, Topic = "Recent Topic", WikipediaUrl = "url2", FirstAttemptDate = DateTime.UtcNow.AddDays(-1), LastAttemptDate = DateTime.UtcNow.AddDays(-1), LastScore = 15, BestScore = 15, TotalQuestions = 20, Type = "Quiz" }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetStatisticsAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.LastActivity);
        Assert.Equal("Recent Topic", result.LastActivity.Topic);
    }
    
    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnBestScore()
    {
        // Arrange
        var userId = "test-user-011";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 11", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activities = new List<UserActivity>
        {
            new UserActivity { UserId = userId, Topic = "Low Score", WikipediaUrl = "url1", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 5, BestScore = 5, TotalQuestions = 10, Type = "Quiz" },
            new UserActivity { UserId = userId, Topic = "High Score", WikipediaUrl = "url2", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 18, BestScore = 18, TotalQuestions = 20, Type = "Quiz" }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetStatisticsAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BestScore);
        Assert.Equal("High Score", result.BestScore.Topic);
        Assert.Equal(18, result.BestScore.Score);
        Assert.Equal(20, result.BestScore.TotalQuestions);
        Assert.Equal(90.0, result.BestScore.Percentage);
    }
    
    [Fact]
    public async Task GetActivityCalendarAsync_ShouldGroupByDate()
    {
        // Arrange
        var userId = "test-user-012";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 12", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var today = DateTime.UtcNow.Date;
        var activities = new List<UserActivity>
        {
            new UserActivity { UserId = userId, Topic = "Topic 1", WikipediaUrl = "url1", FirstAttemptDate = today, LastAttemptDate = today, LastScore = 10, BestScore = 10, TotalQuestions = 10, Type = "Quiz" },
            new UserActivity { UserId = userId, Topic = "Topic 2", WikipediaUrl = "url2", FirstAttemptDate = today, LastAttemptDate = today, LastScore = 15, BestScore = 15, TotalQuestions = 20, Type = "Quiz" },
            new UserActivity { UserId = userId, Topic = "Topic 3", WikipediaUrl = "url3", FirstAttemptDate = today.AddDays(-1), LastAttemptDate = today.AddDays(-1), LastScore = 8, BestScore = 8, TotalQuestions = 10, Type = "Quiz" }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetActivityCalendarAsync(userId, 7);
        
        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        
        var todayEntry = resultList.FirstOrDefault(r => r.Date.Date == today);
        Assert.NotNull(todayEntry);
        Assert.Equal(2, todayEntry.Count);
        
        var yesterdayEntry = resultList.FirstOrDefault(r => r.Date.Date == today.AddDays(-1));
        Assert.NotNull(yesterdayEntry);
        Assert.Equal(1, yesterdayEntry.Count);
    }
    
    [Fact]
    public async Task GetTopScoresAsync_ShouldReturnTopN_OrderedByPercentage()
    {
        // Arrange
        var userId = "test-user-013";
        var user = new User 
        { 
            Id = userId, 
            Name = "Test User 13", 
            CreatedAt = DateTime.UtcNow, 
            LastConnectionAt = DateTime.UtcNow 
        };
        
        var activities = new List<UserActivity>
        {
            new UserActivity { UserId = userId, Topic = "Perfect Score", WikipediaUrl = "url1", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 10, BestScore = 10, TotalQuestions = 10, Type = "Quiz" },
            new UserActivity { UserId = userId, Topic = "High Score", WikipediaUrl = "url2", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 18, BestScore = 18, TotalQuestions = 20, Type = "Quiz" },
            new UserActivity { UserId = userId, Topic = "Low Score", WikipediaUrl = "url3", FirstAttemptDate = DateTime.UtcNow, LastAttemptDate = DateTime.UtcNow, LastScore = 5, BestScore = 5, TotalQuestions = 10, Type = "Quiz" }
        };
        
        _context.Users.Add(user);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetTopScoresAsync(userId, 2);
        
        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("Perfect Score", resultList[0].Topic);
        Assert.Equal(100.0, resultList[0].Percentage);
        Assert.Equal("High Score", resultList[1].Topic);
        Assert.Equal(90.0, resultList[1].Percentage);
    }
    
    #endregion
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
