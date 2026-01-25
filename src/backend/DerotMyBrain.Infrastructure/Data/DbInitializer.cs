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
        if (await context.Users.AnyAsync(u => u.Id == "test-user-id"))
        {
            return; // DB has already been seeded
        }

        // Create Test User
        var allCategories = await categoryService.GetAllCategoriesAsync();
        
        var testUser = new User
        {
            Id = "test-user-id",
            Name = "TestUser",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = "test-user-id",
                QuestionsPerQuiz = 10,
                Theme = "derot-brain",
                Language = "en",
                FavoriteCategories = allCategories.Take(5).ToList()
            }
        };

        context.Users.Add(testUser);
        
        // Topic Hashes
        var quantumHash = SourceHasher.GenerateHash(WikiType, "https://en.wikipedia.org/wiki/Quantum_mechanics");
        var relativityHash = SourceHasher.GenerateHash(WikiType, "https://en.wikipedia.org/wiki/Theory_of_relativity");
        var aiHash = SourceHasher.GenerateHash(WikiType, "https://en.wikipedia.org/wiki/Artificial_intelligence");

        var baseDate = DateTime.UtcNow;

        // Seed realistic activities
        var activities = new List<UserActivity>
        {
            // ===== Topic Evolution: Quantum Mechanics =====
            new UserActivity
            {
                UserId = "test-user-id",
                Type = ActivityType.Read,
                Title = "Quantum Mechanics",
                Description = "Getting to know the basics of Quantum Physics.",
                SourceId = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                SourceType = WikiType,
                SourceHash = quantumHash,
                SessionDateStart = baseDate.AddDays(-5).AddHours(-1),
                SessionDateEnd = baseDate.AddDays(-5),
                ReadDurationSeconds = 1200,
                IsCompleted = true
            },
            new UserActivity
            {
                UserId = "test-user-id",
                Type = ActivityType.Quiz,
                Title = "Quantum Mechanics",
                Description = "First evaluation of basic concepts.",
                SourceId = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                SourceType = WikiType,
                SourceHash = quantumHash,
                SessionDateStart = baseDate.AddDays(-3).AddHours(-0.5),
                SessionDateEnd = baseDate.AddDays(-3),
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
                UserId = "test-user-id",
                Type = ActivityType.Quiz,
                Title = "Quantum Mechanics",
                Description = "Re-evaluation after further study.",
                SourceId = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                SourceType = WikiType,
                SourceHash = quantumHash,
                SessionDateStart = baseDate.AddMinutes(-45),
                SessionDateEnd = baseDate,
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
                UserId = "test-user-id",
                Type = ActivityType.Quiz,
                Title = "Theory of Relativity",
                Description = "General relativity quiz.",
                SourceId = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                SourceType = WikiType,
                SourceHash = relativityHash,
                SessionDateStart = baseDate.AddDays(-1).AddHours(-1),
                SessionDateEnd = baseDate.AddDays(-1),
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

        // Seed focal points
        var userFocuses = new List<UserFocus>
        {
            new UserFocus
            {
                UserId = "test-user-id",
                SourceHash = quantumHash,
                SourceId = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                SourceType = WikiType,
                DisplayTitle = "Quantum Mechanics Mastery",
                BestScore = 90.0,
                LastScore = 90.0,
                LastAttemptDate = baseDate,
                TotalReadTimeSeconds = 1200 + 300 + 600,
                TotalQuizTimeSeconds = 600 + 450,
                TotalStudyTimeSeconds = 1200 + 300 + 600 + 600 + 450
            }
        };

        context.UserFocuses.AddRange(userFocuses);

        await context.SaveChangesAsync();
    }
}
