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
            Name = "test-user-integration",
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
        
        var physicsSource = new Source
        {
            Id = SourceHasher.GenerateId(wikiType, "https://en.wikipedia.org/wiki/Physics"),
            UserId = userId,
            Type = wikiType,
            ExternalId = "Physics",
            DisplayTitle = "Physics",
            IsTracked = true
        };
        
        var historySource = new Source
        {
            Id = SourceHasher.GenerateId(wikiType, "https://en.wikipedia.org/wiki/History"),
            UserId = userId,
            Type = wikiType,
            ExternalId = "History",
            DisplayTitle = "History"
        };

        var physicsOnlineResource = new OnlineResource
        {
             UserId = userId,
             SourceId = physicsSource.Id,
             URL = "https://en.wikipedia.org/wiki/Physics",
             Title = "Physics",
             Provider = "Wikipedia",
             SavedAt = DateTime.UtcNow
        };
        
        var historyOnlineResource = new OnlineResource
        {
             UserId = userId,
             SourceId = historySource.Id,
             URL = "https://en.wikipedia.org/wiki/History",
             Title = "History",
             Provider = "Wikipedia",
             SavedAt = DateTime.UtcNow
        };

        physicsSource.OnlineResource = physicsOnlineResource;
        historySource.OnlineResource = historyOnlineResource;

        var physicsSession = new UserSession
        {
            Id = "session-physics",
            UserId = userId,
            TargetSourceId = physicsSource.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-60),
            Status = SessionStatus.Stopped
        };

        var historySession = new UserSession
        {
            Id = "session-history",
            UserId = userId,
            TargetSourceId = historySource.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            Status = SessionStatus.Stopped
        };

        var activity1 = new UserActivity
        {
            Id = "activity-1",
            UserId = userId,
            UserSessionId = physicsSession.Id,
            Title = "Physics",
            Description = "Quiz on Physics",
            Type = ActivityType.Quiz,
            SessionDateStart = physicsSession.StartedAt,
            SessionDateEnd = DateTime.UtcNow.AddMinutes(-50),
            DurationSeconds = 600,
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
            UserSessionId = historySession.Id,
            Title = "History",
            Description = "Reading History",
            Type = ActivityType.Read,
            SessionDateStart = historySession.StartedAt,
            SessionDateEnd = DateTime.UtcNow,
            DurationSeconds = 600,
            IsCompleted = true
        };

        context.Users.Add(testUser);
        context.Sources.AddRange(physicsSource, historySource);
        context.Sessions.AddRange(physicsSession, historySession);
        context.Activities.AddRange(activity1, activity2);
        
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
        context.Sessions.RemoveRange(context.Sessions);
        context.OnlineResources.RemoveRange(context.OnlineResources);
        context.Sources.RemoveRange(context.Sources);
        context.UserPreferences.RemoveRange(context.UserPreferences);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }
}
