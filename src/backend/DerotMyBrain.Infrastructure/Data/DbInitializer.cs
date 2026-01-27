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
        User? testUser = await context.Users.Include(u => u.Preferences).FirstOrDefaultAsync(u => u.Id == testUserId);

        if (testUser == null)
        {
            // Create Test User
            var allCategories = await categoryService.GetAllCategoriesAsync();

            testUser = new User
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
            await context.SaveChangesAsync();
        }

        // --- SEED SOURCES ---
        
        var qmId = SourceHasher.GenerateId(WikiType, "Quantum_mechanics");
        var quantumSource = await context.Sources.FindAsync(qmId);
        if (quantumSource == null)
        {
            quantumSource = new Source
            {
                Id = qmId,
                UserId = testUserId,
                Type = WikiType,
                ExternalId = "Quantum_mechanics",
                DisplayTitle = "Quantum Mechanics",
                Url = "https://en.wikipedia.org/wiki/Quantum_mechanics",
                IsTracked = true
            };
            context.Sources.Add(quantumSource);
        }
        
        var relId = SourceHasher.GenerateId(WikiType, "Theory_of_relativity");
        var relativitySource = await context.Sources.FindAsync(relId);
        if (relativitySource == null)
        {
            relativitySource = new Source
            {
                Id = relId,
                UserId = testUserId,
                Type = WikiType,
                ExternalId = "Theory_of_relativity",
                DisplayTitle = "Theory of Relativity",
                Url = "https://en.wikipedia.org/wiki/Theory_of_relativity",
                IsTracked = false
            };
            context.Sources.Add(relativitySource);
        }
        
        await context.SaveChangesAsync();

        // --- SEED ADDITIONAL ACTIVITIES ---
        await SeedAdditionalActivitiesAsync(context, testUserId, quantumSource.Id, relativitySource.Id);
    }

    private static async Task SeedAdditionalActivitiesAsync(DerotDbContext context, string userId, string qmSourceId, string relSourceId)
    {
        var baseDate = DateTime.UtcNow;

        // --- Quantum Mechanics Progression ---
        
        // 1. First Attempt (Baseline) - 60%
        if (!await context.Activities.AnyAsync(a => a.UserId == userId && a.SourceId == qmSourceId && a.Score == 6))
        {
            var session = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-10), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-10).AddMinutes(30) };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            context.Activities.Add(new UserActivity
            {
                UserId = userId, UserSessionId = session.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
                Title = "Quantum Mechanics", Description = "Première évaluation.",
                SessionDateStart = session.StartedAt, SessionDateEnd = session.EndedAt,
                Score = 6, QuestionCount = 10, ScorePercentage = 60.0, IsBaseline = true, IsNewBestScore = true, IsCompleted = true
            });
        }

        // 2. Improvement (New Record) - 80%
        if (!await context.Activities.AnyAsync(a => a.UserId == userId && a.SourceId == qmSourceId && a.Score == 8))
        {
            var session = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-5), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-5).AddMinutes(20) };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            context.Activities.Add(new UserActivity
            {
                UserId = userId, UserSessionId = session.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
                Title = "Quantum Mechanics", Description = "Progression constatée.",
                SessionDateStart = session.StartedAt, SessionDateEnd = session.EndedAt,
                Score = 8, QuestionCount = 10, ScorePercentage = 80.0, IsBaseline = false, IsNewBestScore = true, IsCompleted = true
            });
        }

        // 3. Perfect Score (Current Best) - 100%
        if (!await context.Activities.AnyAsync(a => a.UserId == userId && a.SourceId == qmSourceId && a.Score == 10))
        {
            var session = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-2), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-2).AddMinutes(15) };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            context.Activities.Add(new UserActivity
            {
                UserId = userId, UserSessionId = session.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
                Title = "Quantum Mechanics", Description = "Maîtrise totale !",
                SessionDateStart = session.StartedAt, SessionDateEnd = session.EndedAt,
                Score = 10, QuestionCount = 10, ScorePercentage = 100.0, IsBaseline = false, IsNewBestScore = true, IsCompleted = true
            });
        }

        // 4. Maintenance (Standard Attempt) - 90%
        if (!await context.Activities.AnyAsync(a => a.UserId == userId && a.SourceId == qmSourceId && a.Score == 9))
        {
            var session = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-1), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-1).AddMinutes(10) };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            context.Activities.Add(new UserActivity
            {
                UserId = userId, UserSessionId = session.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
                Title = "Quantum Mechanics", Description = "Entraînement régulier.",
                SessionDateStart = session.StartedAt, SessionDateEnd = session.EndedAt,
                Score = 9, QuestionCount = 10, ScorePercentage = 90.0, IsBaseline = false, IsNewBestScore = false, IsCompleted = true
            });
        }

        // --- Theory of Relativity Progression ---

        // 1. Initial Read
        if (!await context.Activities.AnyAsync(a => a.UserId == userId && a.SourceId == relSourceId && a.Type == ActivityType.Read))
        {
            var session = new UserSession { UserId = userId, TargetSourceId = relSourceId, StartedAt = baseDate.AddDays(-8), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-8).AddMinutes(45) };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            context.Activities.Add(new UserActivity
            {
                UserId = userId, UserSessionId = session.Id, SourceId = relSourceId, Type = ActivityType.Read,
                Title = "Theory of Relativity", Description = "Lecture approfondie.",
                SessionDateStart = session.StartedAt, SessionDateEnd = session.EndedAt,
                ReadDurationSeconds = 2700, IsCompleted = true
            });
        }

        // 2. High Score (Current Best) - 90%
        if (!await context.Activities.AnyAsync(a => a.UserId == userId && a.SourceId == relSourceId && a.Score == 9))
        {
            var session = new UserSession { UserId = userId, TargetSourceId = relSourceId, StartedAt = baseDate.AddDays(-3), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-3).AddMinutes(25) };
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            context.Activities.Add(new UserActivity
            {
                UserId = userId, UserSessionId = session.Id, SourceId = relSourceId, Type = ActivityType.Quiz,
                Title = "Theory of Relativity", Description = "Excellent résultat.",
                SessionDateStart = session.StartedAt, SessionDateEnd = session.EndedAt,
                Score = 9, QuestionCount = 10, ScorePercentage = 90.0, IsBaseline = true, IsNewBestScore = true, IsCompleted = true
            });
        }

        await context.SaveChangesAsync();
    }
}
