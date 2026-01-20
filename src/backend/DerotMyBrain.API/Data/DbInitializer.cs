using DerotMyBrain.API.Models;
using DerotMyBrain.API.Services;
using Microsoft.EntityFrameworkCore;

namespace DerotMyBrain.API.Data;

/// <summary>
/// Initializes the database with seed data.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(DerotDbContext context, ICategoryService categoryService)
    {
        // Check if any users exist
        if (await context.Users.AnyAsync())
        {
            return; // DB has been seeded
        }

        // Create Test User
        var allCategories = await categoryService.GetAllCategoriesAsync();
        
        var testUser = new User
        {
            Id = "test-user-id",
            Name = "Test User",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = "test-user-id",
                QuestionCount = 10,
                PreferredTheme = "derot-brain",
                Language = "auto",
                SelectedCategories = allCategories.Select(c => c.Id).ToList()
            }
        };

        context.Users.Add(testUser);
        
        // Add some sample activities for the test user
        var activities = new List<UserActivity>
        {
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "History",
                WikipediaUrl = "https://en.wikipedia.org/wiki/History",
                Type = "Read",
                FirstAttemptDate = DateTime.UtcNow.AddDays(-5),
                LastAttemptDate = DateTime.UtcNow.AddDays(-5),
                IsTracked = true
            },
            new UserActivity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Topic = "Physics",
                WikipediaUrl = "https://en.wikipedia.org/wiki/Physics",
                Type = "Quiz",
                FirstAttemptDate = DateTime.UtcNow.AddDays(-2),
                LastAttemptDate = DateTime.UtcNow.AddDays(-2),
                BestScore = 8,
                TotalQuestions = 10,
                IsTracked = true
            }
        };

        context.Activities.AddRange(activities);

        await context.SaveChangesAsync();
    }
}
