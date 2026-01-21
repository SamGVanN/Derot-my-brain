using DerotMyBrain.API.Models;
using DerotMyBrain.API.Services;
using Microsoft.EntityFrameworkCore;

namespace DerotMyBrain.API.Data;

/// <summary>
/// Initializes the database with seed data for development.
/// </summary>
public static class DbInitializer
{
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
                QuestionCount = 10,
                PreferredTheme = "derot-brain",
                Language = "en",
                SelectedCategories = allCategories.Select(c => c.Id).ToList()
            }
        };

        context.Users.Add(testUser);
        
        // Seed realistic activities
        var baseDate = DateTime.UtcNow;
        var activities = new List<UserActivity>
        {
            // ===== Topic Evolution: Quantum Mechanics (Tracked) =====
            // Session 1: Read (5 days ago)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Read",
                SessionDate = baseDate.AddDays(-5),
                Score = null,
                TotalQuestions = null
            },
            // Session 2: First quiz (3 days ago, 6/10)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-3),
                Score = 6,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0"
            },
            // Session 3: Second quiz (today, 9/10 - improvement!)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Quiz",
                SessionDate = baseDate,
                Score = 9,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0"
            },

            // ===== Non-Tracked Topic: Theory of Relativity =====
            // Session 1: Read (2 days ago)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Theory of Relativity",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Read",
                SessionDate = baseDate.AddDays(-2),
                Score = null,
                TotalQuestions = null
            },
            // Session 2: Quiz (1 day ago, 7/10)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Theory of Relativity",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-1),
                Score = 7,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0"
            },

            // ===== Tracked Topic with only reads: Artificial Intelligence =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Artificial Intelligence",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
                Type = "Read",
                SessionDate = baseDate.AddDays(-7),
                Score = null,
                TotalQuestions = null
            },

            // ===== More activities to show history paginated =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "World War II",
                WikipediaUrl = "https://en.wikipedia.org/wiki/World_War_II",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-10),
                Score = 8,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b"
            }
        };

        context.Activities.AddRange(activities);

        // Seed Tracked Topics
        var trackedTopics = new List<TrackedTopic>
        {
            // Quantum Mechanics (Tracked 4 days ago)
            new TrackedTopic
            {
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                TrackedDate = baseDate.AddDays(-4),
                TotalReadSessions = 1,
                TotalQuizAttempts = 2,
                FirstReadDate = baseDate.AddDays(-5),
                LastReadDate = baseDate.AddDays(-5),
                FirstAttemptDate = baseDate.AddDays(-3),
                LastAttemptDate = baseDate,
                BestScore = 9,
                TotalQuestions = 10,
                BestScoreDate = baseDate
            },
            // Artificial Intelligence (Tracked 7 days ago)
            new TrackedTopic
            {
                UserId = "test-user-id",
                Topic = "Artificial Intelligence",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
                TrackedDate = baseDate.AddDays(-7),
                TotalReadSessions = 1,
                TotalQuizAttempts = 0,
                FirstReadDate = baseDate.AddDays(-7),
                LastReadDate = baseDate.AddDays(-7),
                FirstAttemptDate = null,
                LastAttemptDate = null,
                BestScore = null,
                TotalQuestions = null,
                BestScoreDate = null
            }
        };

        context.TrackedTopics.AddRange(trackedTopics);

        await context.SaveChangesAsync();

    }
}
