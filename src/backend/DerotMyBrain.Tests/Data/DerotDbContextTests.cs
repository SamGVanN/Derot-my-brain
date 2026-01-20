using Microsoft.EntityFrameworkCore;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.Models;
using Xunit;

namespace DerotMyBrain.Tests.Data;

public class DerotDbContextTests : IDisposable
{
    private readonly DerotDbContext _context;
    
    public DerotDbContextTests()
    {
        var options = new DbContextOptionsBuilder<DerotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new DerotDbContext(options);
    }
    
    [Fact]
    public async Task CanAddAndRetrieveUser()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user-001",
            Name = "Test User",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow
        };
        
        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var retrieved = await _context.Users.FindAsync("test-user-001");
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test User", retrieved.Name);
    }
    
    [Fact]
    public async Task CanAddUserWithPreferences()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user-002",
            Name = "Test User 2",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = "test-user-002",
                QuestionCount = 15,
                PreferredTheme = "knowledge-core",
                Language = "fr"
            }
        };
        
        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var retrieved = await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == "test-user-002");
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Preferences);
        Assert.Equal(15, retrieved.Preferences.QuestionCount);
    }
    
    [Fact]
    public async Task CanAddUserActivity()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user-003",
            Name = "Test User 3",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow
        };
        
        var activity = new UserActivity
        {
            UserId = user.Id,
            Topic = "Quantum Mechanics",
            WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
            FirstAttemptDate = DateTime.UtcNow,
            LastAttemptDate = DateTime.UtcNow,
            LastScore = 18,
            BestScore = 18,
            TotalQuestions = 20,
            LlmModelName = "llama3:8b",
            LlmVersion = "v1.0",
            IsTracked = true,
            Type = "Quiz"
        };
        
        // Act
        _context.Users.Add(user);
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        
        var retrieved = await _context.Activities
            .FirstOrDefaultAsync(a => a.Topic == "Quantum Mechanics");
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(18, retrieved.BestScore);
        Assert.True(retrieved.IsTracked);
    }
    
    [Fact]
    public async Task CascadeDeleteRemovesActivities()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user-004",
            Name = "Test User 4",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow
        };
        
        var activity = new UserActivity
        {
            UserId = user.Id,
            Topic = "Test Topic",
            WikipediaUrl = "https://test.com",
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
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        var activityExists = await _context.Activities.AnyAsync(a => a.UserId == "test-user-004");
        
        // Assert
        Assert.False(activityExists);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
