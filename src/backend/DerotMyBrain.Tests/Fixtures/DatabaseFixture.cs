using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Tests.Integration;

namespace DerotMyBrain.Tests.Fixtures;

/// <summary>
/// Provides database setup and seeding utilities for integration tests.
/// Helps ensure test isolation and reusable test data creation.
/// </summary>
public class DatabaseFixture
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseFixture(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Seeds the database with default test data (user + activities).
    /// </summary>
    public async Task SeedDefaultTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed all data in one transaction to ensure it's persisted
        const string userId = "test-user-integration";
        
        // Check if data already exists
        if (await context.Users.FindAsync(userId) != null)
            return;
        
        // Create user with preferences
        var testUser = new User
        {
            Id = userId,
            Name = "Integration Test User",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = userId,
                QuestionsPerQuiz = 10,
                Theme = "derot-brain",
                Language = "en",
                FavoriteCategories = new List<WikipediaCategory>()
            }
        };

        // Create activities
        var activity1 = new UserActivity
        {
            Id = "activity-1",
            UserId = userId,
            Title = "Physics",
            Description = "Quiz on Physics",
            SourceUrl = "https://en.wikipedia.org/wiki/Physics",
            Type = "Quiz",
            LastAttemptDate = DateTime.UtcNow.AddDays(-5),
            Score = 8,
            MaxScore = 10,
            IsTracked = true
        };

        var activity2 = new UserActivity
        {
            Id = "activity-2",
            UserId = userId,
            Title = "History",
            Description = "Reading History",
            SourceUrl = "https://en.wikipedia.org/wiki/History",
            Type = "Read",
            LastAttemptDate = DateTime.UtcNow.AddDays(-2),
            Score = 0,
            MaxScore = 0,
            IsTracked = false
        };

        // Create tracked topics
        var trackedTopic1 = new TrackedTopic
        {
            UserId = userId,
            Title = "Physics",
            // WikipediaUrl removed from TrackedTopic
            // TrackedDate removed
            LastInteraction = DateTime.UtcNow.AddDays(-5),
            BestScore = 8,
            // TotalQuestions removed? No, TrackedTopic has stats but usually aggregated.
            // Entity has: BestScore, BestScoreDate, TotalQuizAttempts, TotalReadSessions
            BestScoreDate = DateTime.UtcNow.AddDays(-5),
            TotalQuizAttempts = 1,
            TotalReadSessions = 0
        };

        // Add all entities
        context.Users.Add(testUser);
        context.Activities.AddRange(activity1, activity2);
        context.TrackedTopics.Add(trackedTopic1);
        
        // Save all in one transaction
        await context.SaveChangesAsync();

    }

    /// <summary>
    /// Seeds a test user with preferences.
    /// </summary>
    public async Task SeedTestUserAsync(DerotDbContext context, string userId = "test-user-integration")
    {
        // Check if user already exists
        if (await context.Users.FindAsync(userId) != null)
            return;

        var testUser = new User
        {
            Id = userId,
            Name = "Integration Test User",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = userId,
                QuestionsPerQuiz = 10,
                Theme = "derot-brain",
                Language = "en",
                FavoriteCategories = new List<WikipediaCategory>()
            }
        };

        context.Users.Add(testUser);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds test activities for the default test user.
    /// </summary>
    public async Task SeedTestActivitiesAsync(DerotDbContext context, string userId = "test-user-integration")
    {
        var activity1 = new UserActivity
        {
            Id = "activity-1",
            UserId = userId,
            Title = "Physics",
            Description = "Quiz on Physics",
            SourceUrl = "https://en.wikipedia.org/wiki/Physics",
            Type = "Quiz",
            LastAttemptDate = DateTime.UtcNow.AddDays(-5),
            Score = 8,
            MaxScore = 10
        };

        var activity2 = new UserActivity
        {
            Id = "activity-2",
            UserId = userId,
            Title = "History",
            Description = "Reading History",
            SourceUrl = "https://en.wikipedia.org/wiki/History",
            Type = "Read",
            LastAttemptDate = DateTime.UtcNow.AddDays(-2),
            Score = 0,
            MaxScore = 0
        };

        context.Activities.AddRange(activity1, activity2);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a custom activity.
    /// </summary>
    public async Task SeedActivityAsync(UserActivity activity)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        
        // Make sure required props are set if passed by builder somewhat incompletely
        // But builder should handle it.
        
        context.Activities.Add(activity);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Cleans up the database (for test isolation).
    /// </summary>
    public async Task CleanupAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        
        // Remove all data
        context.Activities.RemoveRange(context.Activities);
        context.TrackedTopics.RemoveRange(context.TrackedTopics);
        context.UserPreferences.RemoveRange(context.UserPreferences);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a scoped DbContext for custom test scenarios.
    /// </summary>
    public DerotDbContext GetDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DerotDbContext>();
    }
}
