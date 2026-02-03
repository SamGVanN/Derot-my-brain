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
        // Cleanup Test User data for deterministic seeding in development
        const string testUserId = "test-user-id";
        
        // Delete previous activities and sessions for this user to ensure logical progression on every restart
        var previousActivities = context.Activities.Where(a => a.UserId == testUserId);
        context.Activities.RemoveRange(previousActivities);
        
        var previousSessions = context.Sessions.Where(s => s.UserId == testUserId);
        context.Sessions.RemoveRange(previousSessions);

        // Also cleanup sources specifically created for this user (Wikipedia ones)
        var previousSources = context.Sources.Where(s => s.UserId == testUserId);
        context.Sources.RemoveRange(previousSources);

        await context.SaveChangesAsync();

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
        var quantumSource = new Source
        {
            Id = qmId,
            UserId = testUserId,
            Type = WikiType,
            ExternalId = "Quantum_mechanics",
            DisplayTitle = "Quantum Mechanics",
            IsTracked = true
        };
        context.Sources.Add(quantumSource);
        
        var relId = SourceHasher.GenerateId(WikiType, "Theory_of_relativity");
        var relativitySource = new Source
        {
            Id = relId,
            UserId = testUserId,
            Type = WikiType,
            ExternalId = "Theory_of_relativity",
            DisplayTitle = "Theory of Relativity",
            IsTracked = false
        };
        context.Sources.Add(relativitySource);

        // Create OnlineResources for these sources
        context.OnlineResources.Add(new OnlineResource
        {
            UserId = testUserId,
            SourceId = qmId,
            URL = "https://en.wikipedia.org/wiki/Quantum_mechanics",
            Title = "Quantum Mechanics",
            Provider = "Wikipedia",
            SavedAt = DateTime.UtcNow
        });

        context.OnlineResources.Add(new OnlineResource
        {
            UserId = testUserId,
            SourceId = relId,
            URL = "https://en.wikipedia.org/wiki/Theory_of_relativity",
            Title = "Theory of Relativity",
            Provider = "Wikipedia",
            SavedAt = DateTime.UtcNow
        });
        
        await context.SaveChangesAsync();

        // --- SEED ADDITIONAL ACTIVITIES ---
        await SeedAdditionalActivitiesAsync(context, testUserId, quantumSource.Id, relativitySource.Id);
    }

    private static async Task SeedAdditionalActivitiesAsync(DerotDbContext context, string userId, string qmSourceId, string relSourceId)
    {
        // Use fixed dates relative to "now" but strictly ordered
        var baseDate = DateTime.UtcNow;

        // --- Quantum Mechanics Progression ---
        
        // 1. Jan 18: First Attempt (Baseline) - 60%
        var session1 = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-9), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-9).AddMinutes(30) };
        context.Sessions.Add(session1);
        await context.SaveChangesAsync();
        context.Activities.Add(new UserActivity
        {
            UserId = userId, UserSessionId = session1.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
            Title = "Quantum Mechanics", Description = "Première évaluation.",
            SessionDateStart = session1.StartedAt, SessionDateEnd = session1.EndedAt,
            DurationSeconds = 1800,
            Score = 6, QuestionCount = 10, ScorePercentage = 60.0, IsBaseline = true, IsNewBestScore = true, IsCompleted = true
        });

        // 2. Jan 21: Improvement (New Record) - 80%
        var session2 = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-6), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-6).AddMinutes(20) };
        context.Sessions.Add(session2);
        await context.SaveChangesAsync();
        context.Activities.Add(new UserActivity
        {
            UserId = userId, UserSessionId = session2.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
            Title = "Quantum Mechanics", Description = "Progression constatée.",
            SessionDateStart = session2.StartedAt, SessionDateEnd = session2.EndedAt,
            DurationSeconds = 1200,
            Score = 8, QuestionCount = 10, ScorePercentage = 80.0, IsBaseline = false, IsNewBestScore = true, IsCompleted = true
        });

        // 3. Jan 23: Maintenance (NO Record) - 60% (Lower score)
        var session3 = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-4), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-4).AddMinutes(15) };
        context.Sessions.Add(session3);
        await context.SaveChangesAsync();
        context.Activities.Add(new UserActivity
        {
            UserId = userId, UserSessionId = session3.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
            Title = "Quantum Mechanics", Description = "Petite fatigue passante.",
            SessionDateStart = session3.StartedAt, SessionDateEnd = session3.EndedAt,
            DurationSeconds = 900,
            Score = 6, QuestionCount = 10, ScorePercentage = 60.0, IsBaseline = false, IsNewBestScore = false, IsCompleted = true
        });

        // 4. Jan 25: Perfect Score (New Record) - 100%
        var session4 = new UserSession { UserId = userId, TargetSourceId = qmSourceId, StartedAt = baseDate.AddDays(-2), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-2).AddMinutes(25) };
        context.Sessions.Add(session4);
        await context.SaveChangesAsync();
        context.Activities.Add(new UserActivity
        {
            UserId = userId, UserSessionId = session4.Id, SourceId = qmSourceId, Type = ActivityType.Quiz,
            Title = "Quantum Mechanics", Description = "Maîtrise totale !",
            SessionDateStart = session4.StartedAt, SessionDateEnd = session4.EndedAt,
            DurationSeconds = 1500,
            Score = 10, QuestionCount = 10, ScorePercentage = 100.0, IsBaseline = false, IsNewBestScore = true, IsCompleted = true
        });

        // --- Theory of Relativity Progression ---

        // 1. Jan 20: Initial Read
        var sessionRel1 = new UserSession { UserId = userId, TargetSourceId = relSourceId, StartedAt = baseDate.AddDays(-7), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-7).AddMinutes(45) };
        context.Sessions.Add(sessionRel1);
        await context.SaveChangesAsync();
        context.Activities.Add(new UserActivity
        {
            UserId = userId, UserSessionId = sessionRel1.Id, SourceId = relSourceId, Type = ActivityType.Read,
            Title = "Theory of Relativity", Description = "Lecture approfondie.",
            SessionDateStart = sessionRel1.StartedAt, SessionDateEnd = sessionRel1.EndedAt,
            DurationSeconds = 2700, IsCompleted = true
        });

        // 2. Jan 23: First Quiz (Baseline) - 90%
        var sessionRel2 = new UserSession { UserId = userId, TargetSourceId = relSourceId, StartedAt = baseDate.AddDays(-4), Status = SessionStatus.Stopped, EndedAt = baseDate.AddDays(-4).AddMinutes(25) };
        context.Sessions.Add(sessionRel2);
        await context.SaveChangesAsync();
        context.Activities.Add(new UserActivity
        {
            UserId = userId, UserSessionId = sessionRel2.Id, SourceId = relSourceId, Type = ActivityType.Quiz,
            Title = "Theory of Relativity", Description = "Excellent résultat.",
            SessionDateStart = sessionRel2.StartedAt, SessionDateEnd = sessionRel2.EndedAt,
            DurationSeconds = 1500,
            Score = 9, QuestionCount = 10, ScorePercentage = 90.0, IsBaseline = true, IsNewBestScore = true, IsCompleted = true
        });

        await context.SaveChangesAsync();
    }
}
