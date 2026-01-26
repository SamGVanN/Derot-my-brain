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
        
        var quantumSource = new Source
        {
            Id = SourceHasher.GenerateHash(WikiType, "https://en.wikipedia.org/wiki/Quantum_mechanics"),
            Type = WikiType,
            ExternalId = "Quantum_mechanics",
            DisplayTitle = "Quantum Mechanics",
            Url = "https://en.wikipedia.org/wiki/Quantum_mechanics"
        };
        
        var relativitySource = new Source
        {
            Id = SourceHasher.GenerateHash(WikiType, "https://en.wikipedia.org/wiki/Theory_of_relativity"),
            Type = WikiType,
            ExternalId = "Theory_of_relativity",
            DisplayTitle = "Theory of Relativity",
            Url = "https://en.wikipedia.org/wiki/Theory_of_relativity"
        };
        
        context.Sources.AddRange(quantumSource, relativitySource);

        var baseDate = DateTime.UtcNow;

        // --- SEED SESSIONS ---

        var quantumSession1 = new UserSession
        {
            UserId = testUserId,
            SourceId = quantumSource.Id,
            StartedAt = baseDate.AddDays(-5).AddHours(-1),
            EndedAt = baseDate.AddDays(-5),
            Status = SessionStatus.Stopped
        };

        var quantumSession2 = new UserSession
        {
            UserId = testUserId,
            SourceId = quantumSource.Id,
            StartedAt = baseDate.AddDays(-3).AddHours(-0.5),
            EndedAt = baseDate.AddDays(-3),
            Status = SessionStatus.Stopped
        };

        var quantumSession3 = new UserSession
        {
            UserId = testUserId,
            SourceId = quantumSource.Id,
            StartedAt = baseDate.AddMinutes(-45),
            EndedAt = baseDate,
            Status = SessionStatus.Stopped
        };

        var relativitySession = new UserSession
        {
            UserId = testUserId,
            SourceId = relativitySource.Id,
            StartedAt = baseDate.AddDays(-1).AddHours(-1),
            EndedAt = baseDate.AddDays(-1),
            Status = SessionStatus.Stopped
        };

        context.Sessions.AddRange(quantumSession1, quantumSession2, quantumSession3, relativitySession);

        // --- SEED ACTIVITIES ---

        var activities = new List<UserActivity>
        {
            // ===== Topic Evolution: Quantum Mechanics =====
            new UserActivity
            {
                UserId = testUserId,
                UserSessionId = quantumSession1.Id,
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
                UserSessionId = quantumSession3.Id,
                Type = ActivityType.Quiz,
                Title = "Quantum Mechanics",
                Description = "Re-evaluation after further study.",
                SessionDateStart = quantumSession3.StartedAt,
                SessionDateEnd = quantumSession3.EndedAt,
                ReadDurationSeconds = 600,
                QuizDurationSeconds = 450,
                Score = 9,
                QuestionCount = 10,
                ScorePercentage = 90.0,
                IsNewBestScore = true,
                IsCompleted = true
            },

            // ===== Theory of Relativity =====
            new UserActivity
            {
                UserId = testUserId,
                UserSessionId = relativitySession.Id,
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

        // --- SEED FOCUS AREAS ---

        var focusAreas = new List<UserFocus>
        {
            new UserFocus
            {
                UserId = testUserId,
                SourceId = quantumSource.Id,
                DisplayTitle = "Quantum Mechanics Mastery",
                BestScore = 90.0,
                LastScore = 90.0,
                LastAttemptDate = baseDate,
                TotalReadTimeSeconds = 1200 + 300 + 600,
                TotalQuizTimeSeconds = 600 + 450,
                TotalStudyTimeSeconds = 1200 + 300 + 600 + 600 + 450
            }
        };

        context.FocusAreas.AddRange(focusAreas);

        await context.SaveChangesAsync();
    }
}
