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
            // ===== TRACKED QUIZ ACTIVITIES (5) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Mechanics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-180),
                LastAttemptDate = baseDate.AddDays(-15),
                LastScore = 18,
                BestScore = 18,
                TotalQuestions = 20,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0",
                IsTracked = true
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Artificial Intelligence",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-120),
                LastAttemptDate = baseDate.AddDays(-30),
                LastScore = 9,
                BestScore = 9,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0",
                IsTracked = true
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Theory of Relativity",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-90),
                LastAttemptDate = baseDate.AddDays(-10),
                LastScore = 14,
                BestScore = 16,
                TotalQuestions = 20,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0",
                IsTracked = true
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "DNA",
                WikipediaUrl = "https://en.wikipedia.org/wiki/DNA",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-60),
                LastAttemptDate = baseDate.AddDays(-5),
                LastScore = 10,
                BestScore = 10,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0",
                IsTracked = true
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Renaissance Art",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Renaissance_art",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-45),
                LastAttemptDate = baseDate.AddDays(-2),
                LastScore = 13,
                BestScore = 15,
                TotalQuestions = 15,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0",
                IsTracked = true
            },

            // ===== TRACKED READ ACTIVITIES (2) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Blockchain",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Blockchain",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-100),
                LastAttemptDate = baseDate.AddDays(-100),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = true
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Evolution",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Evolution",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-70),
                LastAttemptDate = baseDate.AddDays(-70),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = true
            },

            // ===== NON-TRACKED QUIZ ACTIVITIES (9) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "World War II",
                WikipediaUrl = "https://en.wikipedia.org/wiki/World_War_II",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-150),
                LastAttemptDate = baseDate.AddDays(-150),
                LastScore = 7,
                BestScore = 7,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Ancient Rome",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Ancient_Rome",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-140),
                LastAttemptDate = baseDate.AddDays(-140),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "French Revolution",
                WikipediaUrl = "https://en.wikipedia.org/wiki/French_Revolution",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-110),
                LastAttemptDate = baseDate.AddDays(-110),
                LastScore = 5,
                BestScore = 5,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Mount Everest",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Mount_Everest",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-95),
                LastAttemptDate = baseDate.AddDays(-95),
                LastScore = 8,
                BestScore = 8,
                TotalQuestions = 10,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Sahara Desert",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Sahara",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-80),
                LastAttemptDate = baseDate.AddDays(-80),
                LastScore = 20,
                BestScore = 20,
                TotalQuestions = 20,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Amazon Rainforest",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Amazon_rainforest",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-50),
                LastAttemptDate = baseDate.AddDays(-50),
                LastScore = 6,
                BestScore = 6,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Quantum Computing",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_computing",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-35),
                LastAttemptDate = baseDate.AddDays(-35),
                LastScore = 12,
                BestScore = 12,
                TotalQuestions = 15,
                LlmModelName = "llama3:8b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Classical Music",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Classical_music",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-25),
                LastAttemptDate = baseDate.AddDays(-25),
                LastScore = 4,
                BestScore = 4,
                TotalQuestions = 10,
                LlmModelName = "mistral:7b",
                LlmVersion = "1.0",
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Modern Architecture",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Modern_architecture",
                Type = "Quiz",
                FirstAttemptDate = baseDate.AddDays(-7),
                LastAttemptDate = baseDate.AddDays(-7),
                LastScore = 9,
                BestScore = 9,
                TotalQuestions = 10,
                LlmModelName = "qwen2.5:7b",
                LlmVersion = "1.0",
                IsTracked = false
            },

            // ===== NON-TRACKED READ ACTIVITIES (4) =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Photosynthesis",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Photosynthesis",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-130),
                LastAttemptDate = baseDate.AddDays(-130),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Genetics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Genetics",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-85),
                LastAttemptDate = baseDate.AddDays(-85),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Prime Numbers",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Prime_number",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-55),
                LastAttemptDate = baseDate.AddDays(-55),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Calculus",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Calculus",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-20),
                LastAttemptDate = baseDate.AddDays(-20),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = false
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Probability Theory",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Probability_theory",
                Type = "Read",
                FirstAttemptDate = baseDate.AddDays(-3),
                LastAttemptDate = baseDate.AddDays(-3),
                LastScore = 0,
                BestScore = 0,
                TotalQuestions = 0,
                LlmModelName = null,
                LlmVersion = null,
                IsTracked = false
            }
        };

        context.Activities.AddRange(activities);

        await context.SaveChangesAsync();
    }
}
