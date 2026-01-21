# Implementation Part 1: Core Architecture (Phases 1-4)

**Status:** 🟢 Ready to implement  
**Estimated Time:** 30-45 minutes  
**Prerequisites:** None (first phase)  
**Next:** [`DerotProperties-Prompt-Part2.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties-Prompt-Part2.md) (API Layer)

---

## Overview

This is **Part 1 of 3** in the UserActivity property refactoring. You will implement the core architecture changes:
- Create new `TrackedTopic` model
- Update existing `UserActivity` model
- Database migration
- Repository layer
- Service layer business logic

**Read first:** [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for full context and approved architecture.

---

## Your Task: Build Core Architecture

Implement the **hybrid architecture** foundation:
- `UserActivity` table: Individual sessions (one row per Read/Quiz)
- `TrackedTopic` table: Aggregated summary (tracked topics only)

---

## Phase 1: Model Changes

### 1.1 Create TrackedTopic Model

**File:** `src/backend/DerotMyBrain.API/Models/TrackedTopic.cs`

**Implementation:**
```csharp
namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a topic that the user is tracking for mastery.
/// This is a denormalized cache table for performance.
/// All data can be rebuilt from UserActivity history.
/// </summary>
public class TrackedTopic
{
    /// <summary>
    /// Unique identifier for this tracked topic.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID of the user tracking this topic.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wikipedia topic/article title.
    /// This matches the Topic field in UserActivity.
    /// </summary>
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Full Wikipedia URL for the article.
    /// </summary>
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// When the user first marked this topic as tracked.
    /// Auto-set when tracking is enabled, cannot be changed by user.
    /// </summary>
    public DateTime TrackedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total number of Read sessions for this topic.
    /// Incremented from UserActivity where Type = "Read".
    /// </summary>
    public int TotalReadSessions { get; set; } = 0;
    
    /// <summary>
    /// Total number of Quiz attempts for this topic.
    /// Incremented from UserActivity where Type = "Quiz".
    /// </summary>
    public int TotalQuizAttempts { get; set; } = 0;
    
    /// <summary>
    /// Date of the first Read session for this topic.
    /// Null if user has never read this topic.
    /// </summary>
    public DateTime? FirstReadDate { get; set; }
    
    /// <summary>
    /// Date of the most recent Read session for this topic.
    /// Null if user has never read this topic.
    /// </summary>
    public DateTime? LastReadDate { get; set; }
    
    /// <summary>
    /// Date of the first Quiz attempt on this topic.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public DateTime? FirstAttemptDate { get; set; }
    
    /// <summary>
    /// Date of the most recent Quiz attempt on this topic.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public DateTime? LastAttemptDate { get; set; }
    
    /// <summary>
    /// Best (highest) score achieved across all quiz attempts on this topic.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public int? BestScore { get; set; }
    
    /// <summary>
    /// Total number of questions in the quiz that achieved the best score.
    /// Used to display "BestScore/TotalQuestions" (e.g., "9/10").
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public int? TotalQuestions { get; set; }
    
    /// <summary>
    /// Date when the best score was achieved.
    /// Null if user has never taken a quiz on this topic.
    /// </summary>
    public DateTime? BestScoreDate { get; set; }
    
    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User? User { get; set; }
}
```

**Checklist:**
- [x] File created at correct path
- [x] All properties implemented with correct types
- [x] XML documentation on all properties
- [x] Default values set (`TrackedDate`, counts)

---

### 1.2 Update UserActivity Model

**File:** `src/backend/DerotMyBrain.API/Models/UserActivity.cs`

**Changes Required:**

**REMOVE these properties:**
- `FirstAttemptDate`
- `LastAttemptDate`
- `BestScore`
- `IsTracked`

**ADD this property:**
- `SessionDate` (DateTime, default `DateTime.UtcNow`)

**RENAME:**
- `LastScore` → `Score`

**MAKE NULLABLE:**
- `Score` (int → int?)
- `TotalQuestions` (int → int?)

**CHANGE DEFAULT:**
- `Type` from `"Quiz"` to `"Read"`

**Updated Model:**
```csharp
namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a single user session on a Wikipedia topic.
/// Each row is one discrete session (Read or Quiz).
/// </summary>
public class UserActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of this session: "Read" or "Quiz".
    /// Default is "Read" as reading happens before quizzing.
    /// </summary>
    public string Type { get; set; } = "Read"; // Changed from "Quiz"
    
    /// <summary>
    /// When this session occurred.
    /// </summary>
    public DateTime SessionDate { get; set; } = DateTime.UtcNow; // NEW
    
    /// <summary>
    /// Score from this quiz session.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public int? Score { get; set; } // Renamed from LastScore, made nullable
    
    /// <summary>
    /// Total number of questions in this quiz session.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public int? TotalQuestions { get; set; } // Made nullable
    
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    public User? User { get; set; }
}
```

**Checklist:**
- [x] `FirstAttemptDate` removed
- [x] `LastAttemptDate` removed
- [x] `BestScore` removed
- [x] `IsTracked` removed
- [x] `SessionDate` added with default value
- [x] `LastScore` renamed to `Score`
- [x] `Score` made nullable (int?)
- [x] `TotalQuestions` made nullable (int?)
- [x] `Type` default changed to `"Read"`
- [x] XML documentation updated

---

### 1.3 Update User Model

**File:** `src/backend/DerotMyBrain.API/Models/User.cs`

**ADD navigation property:**
```csharp
public List<TrackedTopic> TrackedTopics { get; set; } = new();
```

**Checklist:**
- [x] Navigation property added

---

## Phase 2: Database Changes

### 2.1 Update DerotDbContext

**File:** `src/backend/DerotMyBrain.API/Data/DerotDbContext.cs`

**Changes:**

1. **Add DbSet:**
```csharp
public DbSet<TrackedTopic> TrackedTopics { get; set; }
```

2. **Update `OnModelCreating` for UserActivity:**
```csharp
modelBuilder.Entity<UserActivity>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // Required fields
    entity.Property(e => e.UserId).IsRequired();
    entity.Property(e => e.Topic).IsRequired();
    entity.Property(e => e.Type).IsRequired().HasDefaultValue("Read");
    entity.Property(e => e.SessionDate).IsRequired();
    
    // Index for user's activity history (most common query)
    entity.HasIndex(e => new { e.UserId, e.SessionDate })
        .HasDatabaseName("IX_UserActivity_UserId_SessionDate");
    
    // Index for topic-specific history (evolution queries)
    entity.HasIndex(e => new { e.UserId, e.Topic, e.SessionDate })
        .HasDatabaseName("IX_UserActivity_UserId_Topic_SessionDate");
    
    // Index for type filtering
    entity.HasIndex(e => new { e.UserId, e.Type, e.SessionDate })
        .HasDatabaseName("IX_UserActivity_UserId_Type_SessionDate");
    
    // Foreign key
    entity.HasOne(e => e.User)
        .WithMany(u => u.Activities)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

3. **Add configuration for TrackedTopic:**
```csharp
modelBuilder.Entity<TrackedTopic>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // Required fields
    entity.Property(e => e.UserId).IsRequired();
    entity.Property(e => e.Topic).IsRequired();
    entity.Property(e => e.TrackedDate).IsRequired();
    
    // Unique constraint: One tracked topic per user
    entity.HasIndex(e => new { e.UserId, e.Topic })
        .IsUnique()
        .HasDatabaseName("IX_TrackedTopic_UserId_Topic_Unique");
    
    // Index for tracked topics list sorted by last attempt
    entity.HasIndex(e => new { e.UserId, e.LastAttemptDate })
        .HasDatabaseName("IX_TrackedTopic_UserId_LastAttemptDate");
    
    // Index for tracked topics list sorted by best score
    entity.HasIndex(e => new { e.UserId, e.BestScore })
        .HasDatabaseName("IX_TrackedTopic_UserId_BestScore");
    
    // Foreign key
    entity.HasOne(e => e.User)
        .WithMany(u => u.TrackedTopics)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

**Checklist:**
- [x] `DbSet<TrackedTopic>` added
- [x] UserActivity indexes updated (removed old ones, added new)
- [x] TrackedTopic configuration added
- [x] Unique constraint on UserId+Topic
- [x] Foreign keys configured

---

### 2.2 Generate and Apply Migration

**Commands:**
```bash
cd src/backend/DerotMyBrain.API
dotnet ef migrations add HybridArchitecture_UserActivity_TrackedTopic
dotnet ef database update
```

**Checklist:**
- [x] Migration generated successfully
- [x] Migration reviewed (verifies drop/create operations)
- [x] Migration applied to database
- [x] No migration errors

---

## Phase 3: Repository Layer

### 3.1 Create ITrackedTopicRepository

**File:** `src/backend/DerotMyBrain.API/Repositories/ITrackedTopicRepository.cs`

```csharp
namespace DerotMyBrain.API.Repositories;

/// <summary>
/// Repository interface for TrackedTopic operations.
/// </summary>
public interface ITrackedTopicRepository
{
    /// <summary>
    /// Gets a tracked topic by its unique identifier.
    /// </summary>
    Task<TrackedTopic?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a tracked topic by user ID and topic name.
    /// </summary>
    Task<TrackedTopic?> GetByTopicAsync(string userId, string topic);
    
    /// <summary>
    /// Gets all tracked topics for a user.
    /// </summary>
    Task<IEnumerable<TrackedTopic>> GetAllAsync(string userId);
    
    /// <summary>
    /// Creates a new tracked topic.
    /// </summary>
    Task<TrackedTopic> CreateAsync(TrackedTopic trackedTopic);
    
    /// <summary>
    /// Updates an existing tracked topic.
    /// </summary>
    Task UpdateAsync(TrackedTopic trackedTopic);
    
    /// <summary>
    /// Deletes a tracked topic by ID.
    /// </summary>
    Task DeleteAsync(string id);
    
    /// <summary>
    /// Checks if a topic is tracked by the user.
    /// </summary>
    Task<bool> ExistsAsync(string userId, string topic);
}
```

**Checklist:**
- [x] Interface created
- [x] All methods defined
- [x] XML documentation on all methods

---

### 3.2 Create SqliteTrackedTopicRepository

**File:** `src/backend/DerotMyBrain.API/Repositories/SqliteTrackedTopicRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Repositories;

/// <summary>
/// SQLite implementation of ITrackedTopicRepository.
/// </summary>
public class SqliteTrackedTopicRepository : ITrackedTopicRepository
{
    private readonly DerotDbContext _context;
    private readonly ILogger<SqliteTrackedTopicRepository> _logger;
    
    public SqliteTrackedTopicRepository(
        DerotDbContext context, 
        ILogger<SqliteTrackedTopicRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<TrackedTopic?> GetByIdAsync(string id)
    {
        try
        {
            return await _context.TrackedTopics
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tracked topic {TrackedTopicId}", id);
            throw;
        }
    }
    
    public async Task<TrackedTopic?> GetByTopicAsync(string userId, string topic)
    {
        try
        {
            return await _context.TrackedTopics
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Topic == topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tracked topic for user {UserId}, topic {Topic}", userId, topic);
            throw;
        }
    }
    
    public async Task<IEnumerable<TrackedTopic>> GetAllAsync(string userId)
    {
        try
        {
            return await _context.TrackedTopics
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.LastAttemptDate ?? t.LastReadDate ?? t.TrackedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tracked topics for user {UserId}", userId);
            throw;
        }
    }
    
    public async Task<TrackedTopic> CreateAsync(TrackedTopic trackedTopic)
    {
        try
        {
            _logger.LogInformation("Creating tracked topic for user {UserId}, topic {Topic}", 
                trackedTopic.UserId, trackedTopic.Topic);
            
            _context.TrackedTopics.Add(trackedTopic);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Tracked topic created: {TrackedTopicId}", trackedTopic.Id);
            return trackedTopic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tracked topic for user {UserId}, topic {Topic}", 
                trackedTopic.UserId, trackedTopic.Topic);
            throw;
        }
    }
    
    public async Task UpdateAsync(TrackedTopic trackedTopic)
    {
        try
        {
            _context.TrackedTopics.Update(trackedTopic);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Tracked topic updated: {TrackedTopicId}", trackedTopic.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tracked topic {TrackedTopicId}", trackedTopic.Id);
            throw;
        }
    }
    
    public async Task DeleteAsync(string id)
    {
        try
        {
            var trackedTopic = await GetByIdAsync(id);
            if (trackedTopic != null)
            {
                _context.TrackedTopics.Remove(trackedTopic);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Tracked topic deleted: {TrackedTopicId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete tracked topic {TrackedTopicId}", id);
            throw;
        }
    }
    
    public async Task<bool> ExistsAsync(string userId, string topic)
    {
        try
        {
            return await _context.TrackedTopics
                .AnyAsync(t => t.UserId == userId && t.Topic == topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if topic exists for user {UserId}, topic {Topic}", userId, topic);
            throw;
        }
    }
}
```

**Checklist:**
- [x] Repository class created
- [x] Constructor with DbContext and ILogger
- [x] All interface methods implemented
- [x] Structured logging on all operations
- [x] Try-catch with error logging
- [x] Async/await throughout

---

### 3.3 Update IActivityRepository

**File:** `src/backend/DerotMyBrain.API/Repositories/IActivityRepository.cs`

**ADD method:**
```csharp
/// <summary>
/// Gets all activities for a specific topic (for evolution tracking).
/// </summary>
Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic);
```

**Checklist:**
- [x] Method added to interface

---

### 3.4 Update SqliteActivityRepository

**File:** `src/backend/DerotMyBrain.API/Repositories/SqliteActivityRepository.cs`

**Implement new method:**
```csharp
public async Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic)
{
    try
    {
        return await _context.Activities
            .Where(a => a.UserId == userId && a.Topic == topic)
            .OrderBy(a => a.SessionDate)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get activities for user {UserId}, topic {Topic}", userId, topic);
        throw;
    }
}
```

**Update any queries using removed properties:**
- Replace `LastAttemptDate` with `SessionDate`
- Remove references to `IsTracked`

**Checklist:**
- [x] `GetAllForTopicAsync` implemented
- [x] Queries updated to use `SessionDate`
- [x] No references to removed properties

---

## Phase 4: Service Layer

### 4.1 Update IActivityService

**File:** `src/backend/DerotMyBrain.API/Services/IActivityService.cs`

**ADD method:**
```csharp
/// <summary>
/// Checks if a topic is tracked by the user.
/// </summary>
Task<bool> IsTopicTrackedAsync(string userId, string topic);
```

**Checklist:**
- [x] Method added to interface

---

### 4.2 Update ActivityService

**File:** `src/backend/DerotMyBrain.API/Services/ActivityService.cs`

**Changes:**

1. **Inject ITrackedTopicRepository:**
```csharp
private readonly IActivityRepository _activityRepository;
private readonly ITrackedTopicRepository _trackedTopicRepository; // NEW
private readonly ILogger<ActivityService> _logger;

public ActivityService(
    IActivityRepository activityRepository,
    ITrackedTopicRepository trackedTopicRepository, // NEW
    ILogger<ActivityService> logger)
{
    _activityRepository = activityRepository;
    _trackedTopicRepository = trackedTopicRepository; // NEW
    _logger = logger;
}
```

2. **Update CreateActivity logic:**
```csharp
public async Task<UserActivity> CreateActivityAsync(string userId, CreateActivityDto dto)
{
    _logger.LogInformation("Creating {Type} activity for user {UserId}, topic {Topic}", 
        dto.Type, userId, dto.Topic);
    
    // Create session record
    var activity = new UserActivity
    {
        UserId = userId,
        Topic = dto.Topic,
        WikipediaUrl = dto.WikipediaUrl,
        Type = dto.Type,
        SessionDate = DateTime.UtcNow,
        Score = dto.Score,
        TotalQuestions = dto.TotalQuestions,
        LlmModelName = dto.LlmModelName,
        LlmVersion = dto.LlmVersion
    };
    
    await _activityRepository.CreateAsync(activity);
    
    // Update TrackedTopic if this topic is tracked
    if (await IsTopicTrackedAsync(userId, dto.Topic))
    {
        await UpdateTrackedTopicCacheAsync(userId, dto.Topic, activity);
    }
    
    _logger.LogInformation("Activity created: {ActivityId}", activity.Id);
    return activity;
}
```

3. **Add helper method to update TrackedTopic cache:**
```csharp
private async Task UpdateTrackedTopicCacheAsync(string userId, string topic, UserActivity newSession)
{
    var tracked = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
    if (tracked == null) return;
    
    if (newSession.Type == "Read")
    {
        tracked.TotalReadSessions++;
        tracked.LastReadDate = newSession.SessionDate;
        if (tracked.FirstReadDate == null)
            tracked.FirstReadDate = newSession.SessionDate;
    }
    else if (newSession.Type == "Quiz")
    {
        tracked.TotalQuizAttempts++;
        tracked.LastAttemptDate = newSession.SessionDate;
        if (tracked.FirstAttemptDate == null)
            tracked.FirstAttemptDate = newSession.SessionDate;
        
        // Update best score if this is a new record
        if (tracked.BestScore == null || newSession.Score > tracked.BestScore)
        {
            tracked.BestScore = newSession.Score;
            tracked.TotalQuestions = newSession.TotalQuestions;
            tracked.BestScoreDate = newSession.SessionDate;
            
            _logger.LogInformation("🎉 New best score for topic {Topic}: {Score}/{Total}", 
                topic, tracked.BestScore, tracked.TotalQuestions);
        }
    }
    
    await _trackedTopicRepository.UpdateAsync(tracked);
}
```

4. **Implement IsTopicTrackedAsync:**
```csharp
public async Task<bool> IsTopicTrackedAsync(string userId, string topic)
{
    return await _trackedTopicRepository.ExistsAsync(userId, topic);
}
```

**Checklist:**
- [x] ITrackedTopicRepository injected
- [x] CreateActivity updated to sync TrackedTopic
- [x] UpdateTrackedTopicCacheAsync helper added
- [x] Logic handles Read vs Quiz sessions
- [x] Best score detection implemented
- [x] IsTopicTrackedAsync implemented
- [x] Structured logging added

---

### 4.3 Create ITrackedTopicService

**File:** `src/backend/DerotMyBrain.API/Services/ITrackedTopicService.cs`

```csharp
using DerotMyBrain.API.DTOs;

namespace DerotMyBrain.API.Services;

/// <summary>
/// Service interface for tracked topic operations.
/// </summary>
public interface ITrackedTopicService
{
    /// <summary>
    /// Tracks a topic for the user. Rebuilds history from existing activities.
    /// </summary>
    Task<TrackedTopicDto> TrackTopicAsync(string userId, string topic, string wikipediaUrl);
    
    /// <summary>
    /// Untracks a topic. Preserves activity history.
    /// </summary>
    Task UntrackTopicAsync(string userId, string topic);
    
    /// <summary>
    /// Gets all tracked topics for a user.
    /// </summary>
    Task<IEnumerable<TrackedTopicDto>> GetAllTrackedTopicsAsync(string userId);
    
    /// <summary>
    /// Gets a specific tracked topic.
    /// </summary>
    Task<TrackedTopicDto?> GetTrackedTopicAsync(string userId, string topic);
}
```

**Checklist:**
- [x] Interface created
- [x] All methods defined
- [x] XML documentation added

---

### 4.4 Create TrackedTopicService

**File:** `src/backend/DerotMyBrain.API/Services/TrackedTopicService.cs`

```csharp
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;

namespace DerotMyBrain.API.Services;

/// <summary>
/// Service for managing tracked topics.
/// </summary>
public class TrackedTopicService : ITrackedTopicService
{
    private readonly ITrackedTopicRepository _trackedTopicRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<TrackedTopicService> _logger;
    
    public TrackedTopicService(
        ITrackedTopicRepository trackedTopicRepository,
        IActivityRepository activityRepository,
        ILogger<TrackedTopicService> logger)
    {
        _trackedTopicRepository = trackedTopicRepository;
        _activityRepository = activityRepository;
        _logger = logger;
    }
    
    public async Task<TrackedTopicDto> TrackTopicAsync(string userId, string topic, string wikipediaUrl)
    {
        _logger.LogInformation("Tracking topic for user {UserId}: {Topic}", userId, topic);
        
        // Check if already tracked (idempotent)
        var existing = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        if (existing != null)
        {
            _logger.LogWarning("Topic {Topic} is already tracked by user {UserId}", topic, userId);
            return MapToDto(existing);
        }
        
        // Create TrackedTopic entry
        var trackedTopic = new TrackedTopic
        {
            UserId = userId,
            Topic = topic,
            WikipediaUrl = wikipediaUrl,
            TrackedDate = DateTime.UtcNow
        };
        
        // Rebuild aggregated data from existing UserActivity history
        var allSessions = await _activityRepository.GetAllForTopicAsync(userId, topic);
        
        foreach (var session in allSessions)
        {
            if (session.Type == "Read")
            {
                trackedTopic.TotalReadSessions++;
                trackedTopic.LastReadDate = session.SessionDate;
                if (trackedTopic.FirstReadDate == null || session.SessionDate < trackedTopic.FirstReadDate)
                    trackedTopic.FirstReadDate = session.SessionDate;
            }
            else if (session.Type == "Quiz")
            {
                trackedTopic.TotalQuizAttempts++;
                trackedTopic.LastAttemptDate = session.SessionDate;
                if (trackedTopic.FirstAttemptDate == null || session.SessionDate < trackedTopic.FirstAttemptDate)
                    trackedTopic.FirstAttemptDate = session.SessionDate;
                
                if (trackedTopic.BestScore == null || session.Score > trackedTopic.BestScore)
                {
                    trackedTopic.BestScore = session.Score;
                    trackedTopic.TotalQuestions = session.TotalQuestions;
                    trackedTopic.BestScoreDate = session.SessionDate;
                }
            }
        }
        
        await _trackedTopicRepository.CreateAsync(trackedTopic);
        
        _logger.LogInformation("Topic tracked: {TrackedTopicId}, rebuilt from {SessionCount} sessions", 
            trackedTopic.Id, allSessions.Count());
        
        return MapToDto(trackedTopic);
    }
    
    public async Task UntrackTopicAsync(string userId, string topic)
    {
        _logger.LogInformation("Untracking topic for user {UserId}: {Topic}", userId, topic);
        
        var tracked = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        if (tracked == null)
        {
            _logger.LogWarning("Topic {Topic} is not tracked by user {UserId}", topic, userId);
            return;
        }
        
        await _trackedTopicRepository.DeleteAsync(tracked.Id);
        
        _logger.LogInformation("Topic untracked: {Topic}", topic);
    }
    
    public async Task<IEnumerable<TrackedTopicDto>> GetAllTrackedTopicsAsync(string userId)
    {
        var trackedTopics = await _trackedTopicRepository.GetAllAsync(userId);
        return trackedTopics.Select(MapToDto);
    }
    
    public async Task<TrackedTopicDto?> GetTrackedTopicAsync(string userId, string topic)
    {
        var trackedTopic = await _trackedTopicRepository.GetByTopicAsync(userId, topic);
        return trackedTopic != null ? MapToDto(trackedTopic) : null;
    }
    
    private static TrackedTopicDto MapToDto(TrackedTopic entity)
    {
        return new TrackedTopicDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Topic = entity.Topic,
            WikipediaUrl = entity.WikipediaUrl,
            TrackedDate = entity.TrackedDate,
            TotalReadSessions = entity.TotalReadSessions,
            TotalQuizAttempts = entity.TotalQuizAttempts,
            FirstReadDate = entity.FirstReadDate,
            LastReadDate = entity.LastReadDate,
            FirstAttemptDate = entity.FirstAttemptDate,
            LastAttemptDate = entity.LastAttemptDate,
            BestScore = entity.BestScore,
            TotalQuestions = entity.TotalQuestions,
            BestScoreDate = entity.BestScoreDate
        };
    }
}
```

**Checklist:**
- [x] Service class created
- [x] All interface methods implemented
- [x] TrackTopic rebuilds history from UserActivity
- [x] Idempotent tracking (checks if already tracked)
- [x] UntrackTopic preserves UserActivity history
- [x] Structured logging throughout
- [x] MapToDto helper implemented

---

## Exit Criteria - Part 1 Complete ✅

Before proceeding to Part 2, verify:

### Code Compilation
- [x] Solution builds without errors
- [x] No compilation warnings related to your changes

### Database
- [x] Migration generated successfully
- [x] Migration applied without errors
- [x] Database schema matches specifications:
  - [x] UserActivity table has SessionDate, nullable Score/TotalQuestions
  - [x] UserActivity table does NOT have FirstAttemptDate, LastAttemptDate, BestScore, IsTracked
  - [x] TrackedTopic table exists with all required fields
  - [x] Indexes created (check with DB tool)
  - [x] Unique constraint on TrackedTopic (UserId, Topic)

### Repository Layer
- [x] ITrackedTopicRepository interface compiles
- [x] SqliteTrackedTopicRepository compiles
- [x] IActivityRepository updated
- [x] SqliteActivityRepository updated

### Service Layer
- [x] IActivityService updated
- [x] ActivityService compiles
- [x] ITrackedTopicService interface compiles
- [x] TrackedTopicService compiles

### Manual Quick Test (Optional)
Run a quick manual test if possible:
```csharp
// Can create these in a test or via minimal API endpoint
var activity = new UserActivity { Type = "Read", Topic = "Test", SessionDate = DateTime.UtcNow };
var tracked = new TrackedTopic { Topic = "Test", TrackedDate = DateTime.UtcNow };
```

---

## Next Steps

Once all exit criteria are met and you've verified core architecture is working:

**👉 Proceed to:** [`DerotProperties-Prompt-Part2.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties-Prompt-Part2.md)

Part 2 will implement:
- DTOs (CreateActivityDto, UserActivityDto, TrackedTopicDto)
- Controllers (ActivitiesController updates, TrackedTopicsController)
- Dependency Injection setup

---

**Need help?** Reference [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for:
- Part 3: Full model specifications
- Part 5: Data flow examples
- Part 6: Index specifications
- Part 7: DTO specifications
