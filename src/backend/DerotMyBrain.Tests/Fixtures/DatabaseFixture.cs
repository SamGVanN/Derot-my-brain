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
        
        var physicsSource = new Source
        {
            Id = SourceHasher.GenerateHash(wikiType, "https://en.wikipedia.org/wiki/Physics"),
            Type = wikiType,
            ExternalId = "Physics",
            DisplayTitle = "Physics",
            Url = "https://en.wikipedia.org/wiki/Physics"
        };
        
        var historySource = new Source
        {
            Id = SourceHasher.GenerateHash(wikiType, "https://en.wikipedia.org/wiki/History"),
            Type = wikiType,
            ExternalId = "History",
            DisplayTitle = "History",
            Url = "https://en.wikipedia.org/wiki/History"
        };

        var physicsSession = new UserSession
        {
            Id = "session-physics",
            UserId = userId,
            SourceId = physicsSource.Id,
            StartedAt = DateTime.UtcNow.AddMinutes(-60),
            Status = SessionStatus.Stopped
        };

        var historySession = new UserSession
        {
            Id = "session-history",
            UserId = userId,
            SourceId = historySource.Id,
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
            UserSessionId = historySession.Id,
            Title = "History",
            Description = "Reading History",
            Type = ActivityType.Read,
            SessionDateStart = historySession.StartedAt,
            SessionDateEnd = DateTime.UtcNow,
            ReadDurationSeconds = 600,
            QuizDurationSeconds = 0,
            IsCompleted = true
        };

        var userFocus1 = new UserFocus
        {
            UserId = userId,
            SourceId = physicsSource.Id,
            DisplayTitle = "Physics",
            LastAttemptDate = DateTime.UtcNow.AddMinutes(-50),
            BestScore = 80.0,
            LastScore = 80.0,
            TotalReadTimeSeconds = 300,
            TotalQuizTimeSeconds = 300,
            TotalStudyTimeSeconds = 600
        };

        context.Users.Add(testUser);
        context.Sources.AddRange(physicsSource, historySource);
        context.Sessions.AddRange(physicsSession, historySession);
        context.Activities.AddRange(activity1, activity2);
        context.FocusAreas.Add(userFocus1);
        
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
        context.Sources.RemoveRange(context.Sources);
        context.FocusAreas.RemoveRange(context.FocusAreas);
        context.UserPreferences.RemoveRange(context.UserPreferences);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }
}
