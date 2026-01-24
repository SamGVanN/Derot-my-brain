using DerotMyBrain.Infrastructure.Data;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DerotMyBrain.Infrastructure.Data;

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
                QuestionsPerQuiz = 10,
                Theme = "derot-brain",
                Language = "en",
                FavoriteCategories = allCategories.Take(5).ToList()
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
                Type = "Reading",
                Title = "Quantum Mechanics",
                Description = "Study session",
                SourceUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                LastAttemptDate = baseDate.AddDays(-5),
                Score = 0,
                MaxScore = 0,
                IsTracked = true
            },
            // Session 2: First quiz (3 days ago, 6/10)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Title = "Quantum Mechanics",
                Description = "Quiz on Quantum Mechanics",
                SourceUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Quiz",
                LastAttemptDate = baseDate.AddDays(-3),
                Score = 6,
                MaxScore = 10,
                IsTracked = true
            },
            // Session 3: Second quiz (today, 9/10 - improvement!)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Title = "Quantum Mechanics",
                Description = "Quiz on Quantum Mechanics",
                SourceUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                Type = "Quiz",
                LastAttemptDate = baseDate,
                Score = 9,
                MaxScore = 10,
                IsTracked = true
            },

            // ===== Non-Tracked Topic: Theory of Relativity =====
            // Session 1: Read (2 days ago)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Title = "Theory of Relativity",
                Description = "Reading session",
                SourceUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Reading",
                LastAttemptDate = baseDate.AddDays(-2),
                Score = 0,
                MaxScore = 0,
                IsTracked = false
            },
            // Session 2: Quiz (1 day ago, 7/10)
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Title = "Theory of Relativity",
                Description = "Quiz on Theory of Relativity",
                SourceUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                Type = "Quiz",
                LastAttemptDate = baseDate.AddDays(-1),
                Score = 7,
                MaxScore = 10,
                IsTracked = false
            },

            // ===== Tracked Topic with only reads: Artificial Intelligence =====
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Title = "Artificial Intelligence",
                Description = "Reading session",
                SourceUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
                Type = "Reading",
                LastAttemptDate = baseDate.AddDays(-7),
                Score = 0,
                MaxScore = 0,
                IsTracked = true
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
                Title = "Quantum Mechanics",
                LastInteraction = baseDate,
                TotalReadSessions = 3,
                BestScore = 8 // Mock score
            },
            // Artificial Intelligence (Tracked 7 days ago)
            new TrackedTopic
            {
                UserId = "test-user-id",
                Title = "Artificial Intelligence",
                LastInteraction = baseDate.AddDays(-7),
                TotalReadSessions = 1,
                BestScore = 2 
            }
        };

        context.TrackedTopics.AddRange(trackedTopics);

        await context.SaveChangesAsync();

    }
}
