using Microsoft.Extensions.DependencyInjection;
using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Utils;
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

    public async Task SeedDefaultTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        
        await context.Database.EnsureCreatedAsync();
        
        const string userId = "test-user-integration";
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

        var wikiType = SourceType.Wikipedia;
        var physicsHash = SourceHasher.GenerateHash(wikiType, "https://en.wikipedia.org/wiki/Physics");
        var historyHash = SourceHasher.GenerateHash(wikiType, "https://en.wikipedia.org/wiki/History");

        var activity1 = new UserActivity
        {
            Id = "activity-1",
            UserId = userId,
            Title = "Physics",
            Description = "Quiz on Physics",
            SourceId = "https://en.wikipedia.org/wiki/Physics",
            SourceType = wikiType,
            SourceHash = physicsHash,
            Type = ActivityType.Quiz,
            SessionDateStart = DateTime.UtcNow.AddMinutes(-60),
            SessionDateEnd = DateTime.UtcNow.AddMinutes(-50),
            ReadDurationSeconds = 300,
            QuizDurationSeconds = 300,
            Score = 8,
            QuestionCount = 10,
            ScorePercentage = 80.0,
            IsNewBestScore = true,
            IsCompleted = true
        };

        var activity2 = new UserActivity
        {
            Id = "activity-2",
            UserId = userId,
            Title = "History",
            Description = "Reading History",
            SourceId = "https://en.wikipedia.org/wiki/History",
            SourceType = wikiType,
            SourceHash = historyHash,
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow.AddMinutes(-10),
            SessionDateEnd = DateTime.UtcNow,
            ReadDurationSeconds = 600,
            QuizDurationSeconds = 0,
            IsCompleted = true
        };

        var userFocus1 = new UserFocus
        {
            UserId = userId,
            SourceId = "https://en.wikipedia.org/wiki/Physics",
            SourceType = wikiType,
            SourceHash = physicsHash,
            DisplayTitle = "Physics",
            LastAttemptDate = DateTime.UtcNow.AddMinutes(-50),
            BestScore = 80.0,
            LastScore = 80.0,
            TotalReadTimeSeconds = 300,
            TotalQuizTimeSeconds = 300,
            TotalStudyTimeSeconds = 600
        };

        context.Users.Add(testUser);
        context.Activities.AddRange(activity1, activity2);
        context.UserFocuses.Add(userFocus1);
        
        await context.SaveChangesAsync();
    }

    public async Task SeedActivityAsync(UserActivity activity)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        context.Activities.Add(activity);
        await context.SaveChangesAsync();
    }

    public async Task CleanupAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        
        context.Activities.RemoveRange(context.Activities);
        context.UserFocuses.RemoveRange(context.UserFocuses);
        context.UserPreferences.RemoveRange(context.UserPreferences);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }
}
