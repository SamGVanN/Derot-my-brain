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
        
        // Seed realistic activities (20 total: 6 Read, 14 Quiz)
        var baseDate = DateTime.UtcNow;
        var activities = new List<UserActivity>
        {
            // ===== TRACKED TOPIC SESSIONS (Quantum Mechanics) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-15),
                Score = 18,
                TotalQuestions = 20,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Read",
                SessionDate = baseDate.AddDays(-180),
                LlmModelName = null,
                LlmVersion = null
            },
            
            // ===== TRACKED TOPIC SESSIONS (Artificial Intelligence) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Artificial Intelligence",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-30),
                Score = 9,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0"
            },
            
            // ===== TRACKED TOPIC SESSIONS (Theory of Relativity) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Theory of Relativity",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-10),
                Score = 14,
                TotalQuestions = 20,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Theory of Relativity",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-90),
                Score = 16,
                TotalQuestions = 20,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0"
            },

            // ===== NON-TRACKED QUIZ ACTIVITIES (9) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "World War II",
                WikipediaUrl = "https://en.wikipedia.org/wiki/World_War_II",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-150),
                Score = 7,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Ancient Rome",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Ancient_Rome",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-140),
                Score = 0,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "French Revolution",
                WikipediaUrl = "https://en.wikipedia.org/wiki/French_Revolution",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-110),
                Score = 5,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Mount Everest",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Mount_Everest",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-95),
                Score = 8,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Sahara Desert",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Sahara",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-80),
                Score = 20,
                TotalQuestions = 20,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Amazon Rainforest",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Amazon_rainforest",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-50),
                Score = 6,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Computing",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_computing",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-35),
                Score = 12,
                TotalQuestions = 15,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Classical Music",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Classical_music",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-25),
                Score = 4,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0"
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Modern Architecture",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Modern_architecture",
                Type = "Quiz",
                SessionDate = baseDate.AddDays(-7),
                Score = 9,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0"
            },

            // ===== NON-TRACKED READ ACTIVITIES (4) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Photosynthesis",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Photosynthesis",
                Type = "Read",
                SessionDate = baseDate.AddDays(-130)
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Genetics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Genetics",
                Type = "Read",
                SessionDate = baseDate.AddDays(-85)
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Prime Numbers",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Prime_number",
                Type = "Read",
                SessionDate = baseDate.AddDays(-55)
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Calculus",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Calculus",
                Type = "Read",
                SessionDate = baseDate.AddDays(-20)
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Probability Theory",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Probability_theory",
                Type = "Read",
                SessionDate = baseDate.AddDays(-3)
            }
        };

        context.Activities.AddRange(activities);

        // Seed Tracked Topics
        var trackedTopics = new List<TrackedTopic>
        {
            new TrackedTopic
            {
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                TrackedDate = baseDate.AddMonths(-2),
                TotalReadSessions = 1,
                TotalQuizAttempts = 1,
                FirstReadDate = baseDate.AddDays(-180),
                LastReadDate = baseDate.AddDays(-180),
                FirstAttemptDate = baseDate.AddDays(-15),
                LastAttemptDate = baseDate.AddDays(-15),
                BestScore = 18,
                TotalQuestions = 20,
                BestScoreDate = baseDate.AddDays(-15)
            },
            new TrackedTopic
            {
                UserId = "test-user-id",
                Topic = "Artificial Intelligence",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
                TrackedDate = baseDate.AddMonths(-1),
                TotalReadSessions = 0,
                TotalQuizAttempts = 1,
                FirstAttemptDate = baseDate.AddDays(-30),
                LastAttemptDate = baseDate.AddDays(-30),
                BestScore = 9,
                TotalQuestions = 10,
                BestScoreDate = baseDate.AddDays(-30)
            },
            new TrackedTopic
            {
                UserId = "test-user-id",
                Topic = "Theory of Relativity",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                TrackedDate = baseDate.AddDays(-10),
                TotalReadSessions = 0,
                TotalQuizAttempts = 2,
                FirstAttemptDate = baseDate.AddDays(-90),
                LastAttemptDate = baseDate.AddDays(-10),
                BestScore = 16,
                TotalQuestions = 20,
                BestScoreDate = baseDate.AddDays(-90)
            }
        };

        context.TrackedTopics.AddRange(trackedTopics);

        await context.SaveChangesAsync();

    }
}
