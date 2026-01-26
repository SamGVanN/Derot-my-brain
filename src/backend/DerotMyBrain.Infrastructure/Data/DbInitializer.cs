using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace DerotMyBrain.Infrastructure.Data;

/// <summary>
/// Initializes the database with seed data for development.
/// </summary>
public static class DbInitializer
{
    private const SourceType WikiType = SourceType.Wikipedia;

    public static async Task InitializeAsync(DerotDbContext context, ICategoryService categoryService)
    {
        // Check if TestUser already exists (idempotency)
        const string testUserId = "test-user-id";
        if (await context.Users.AnyAsync(u => u.Id == testUserId))
        {
            return; // DB has already been seeded
        }

        // Create Test User
        var allCategories = await categoryService.GetAllCategoriesAsync();
        
        var testUser = new User
        {
            Id = testUserId,
            Name = "TestUser",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = testUserId,
                QuestionsPerQuiz = 10,
                Theme = "derot-brain",
                Language = "en",
                FavoriteCategories = allCategories.Take(5).ToList()
            }
        };

        context.Users.Add(testUser);
        
        // --- SEED SOURCES ---
        
        var qmId = SourceHasher.GenerateId(WikiType, "Quantum_mechanics");
        var quantumSource = new Source
        {
            Id = qmId,
            UserId = testUserId,
            Type = WikiType,
            ExternalId = "Quantum_mechanics",
            DisplayTitle = "Quantum Mechanics",
            Url = "https://en.wikipedia.org/wiki/Quantum_mechanics",
            IsTracked = true
        };
        
        var relId = SourceHasher.GenerateId(WikiType, "Theory_of_relativity");
        var relativitySource = new Source
        {
            Id = relId,
            UserId = testUserId,
            Type = WikiType,
            ExternalId = "Theory_of_relativity",
            DisplayTitle = "Theory of Relativity",
            Url = "https://en.wikipedia.org/wiki/Theory_of_relativity",
            IsTracked = false
        };
        
        context.Sources.AddRange(quantumSource, relativitySource);

        var baseDate = DateTime.UtcNow;

        // --- SEED SESSIONS ---

        var quantumSession1 = new UserSession
        {
            UserId = testUserId,
            TargetSourceId = quantumSource.Id,
            StartedAt = baseDate.AddDays(-5).AddHours(-1),
            EndedAt = baseDate.AddDays(-5),
            Status = SessionStatus.Stopped
        };

        var quantumSession2 = new UserSession
        {
            UserId = testUserId,
            TargetSourceId = quantumSource.Id,
            StartedAt = baseDate.AddDays(-3).AddHours(-0.5),
            EndedAt = baseDate.AddDays(-3),
            Status = SessionStatus.Stopped
        };

        var relativitySession = new UserSession
        {
            UserId = testUserId,
            TargetSourceId = relativitySource.Id,
            StartedAt = baseDate.AddDays(-1).AddHours(-1),
            EndedAt = baseDate.AddDays(-1),
            Status = SessionStatus.Stopped
        };

        context.Sessions.AddRange(quantumSession1, quantumSession2, relativitySession);

        // --- SEED ACTIVITIES ---

        var activities = new List<UserActivity>
        {
            new UserActivity
            {
                UserId = testUserId,
                UserSessionId = quantumSession1.Id,
                SourceId = quantumSource.Id,
                Type = ActivityType.Read,
                Title = "Quantum Mechanics",
                Description = "Getting to know the basics of Quantum Physics.",
                SessionDateStart = quantumSession1.StartedAt,
                SessionDateEnd = quantumSession1.EndedAt,
                ReadDurationSeconds = 1200,
                IsCompleted = true
            },
            new UserActivity
            {
                UserId = testUserId,
                UserSessionId = quantumSession2.Id,
                SourceId = quantumSource.Id,
                Type = ActivityType.Quiz,
                Title = "Quantum Mechanics",
                Description = "First evaluation of basic concepts.",
                SessionDateStart = quantumSession2.StartedAt,
                SessionDateEnd = quantumSession2.EndedAt,
                ReadDurationSeconds = 300,
                QuizDurationSeconds = 600,
                Score = 6,
                QuestionCount = 10,
                ScorePercentage = 60.0,
                IsNewBestScore = true,
                IsCompleted = true
            },
            new UserActivity
            {
                UserId = testUserId,
                UserSessionId = relativitySession.Id,
                SourceId = relativitySource.Id,
                Type = ActivityType.Quiz,
                Title = "Theory of Relativity",
                Description = "General relativity quiz.",
                SessionDateStart = relativitySession.StartedAt,
                SessionDateEnd = relativitySession.EndedAt,
                ReadDurationSeconds = 1800,
                QuizDurationSeconds = 900,
                Score = 7,
                QuestionCount = 10,
                ScorePercentage = 70.0,
                IsNewBestScore = true,
                IsCompleted = true
            }
        };

        context.Activities.AddRange(activities);

        await context.SaveChangesAsync();
    }
}
