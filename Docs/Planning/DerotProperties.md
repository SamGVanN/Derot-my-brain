# UserActivity Model Property Inconsistencies & Architecture Fix

> **Status:** APPROVED - Ready for implementation  
> **Approved by:** User  
> **Date:** 2026-01-21  
> **Migration Strategy:** Ignore existing data (development phase)

---

## Executive Summary

The current `UserActivity` model has **fundamental architectural issues** that prevent it from meeting the functional specifications:

1. **❌ Schema Mismatch**: Specs require "one row per session", but current model aggregates multiple attempts into one row per topic
2. **❌ Non-nullable Properties**: Dates and scores are non-nullable, causing issues for "Read" activities without quiz attempts
3. **❌ Missing Session History**: Cannot track individual attempt evolution for the same topic
4. **❌ No Performance Optimization**: Tracked Topics queries scan entire Activities table

**✅ APPROVED SOLUTION: Hybrid Architecture**
- **`UserActivity` Table**: One row per session (Read or Quiz) - source of truth
- **`TrackedTopic` Table**: Cached aggregated data - performance optimization

---

## Part 1: Current Property Inconsistencies

### Critical Issues Summary

| Property | Current Type | Issue | Required Change |
|----------|-------------|-------|-----------------|
| `FirstAttemptDate` | `DateTime` | Not applicable for "Read" activities | **REMOVE** (move to TrackedTopic) |
| `LastAttemptDate` | `DateTime` | Not applicable for "Read" activities | **REMOVE** (move to TrackedTopic) |
| `LastScore` | `int` | Ambiguous (0 = no quiz or 0/10?) | Rename to `Score`, make `int?` |
| `BestScore` | `int` | Aggregated value, not per-session | **REMOVE** (move to TrackedTopic) |
| `TotalQuestions` | `int` | Not applicable for "Read" activities | Make `int?` |
| `Type` default | `"Quiz"` | Wrong default value | Change to `"Read"` |
| `IsTracked` | `bool` | Should rely on TrackedTopic table | **REMOVE** |
| *(Missing)* | N/A | Need session timestamp | **ADD** `SessionDate` |

---

## Part 2: Why Current Schema Doesn't Work

### Specification Requirement (Section 1.3.7)

> "Liste chronologique immuable de toutes les sessions. **Chaque entrée correspond à une session unique** (Lecture ou Quiz)."

**Current Implementation Problem:**
```
User reads "Quantum Mechanics" → One row created
User takes quiz, scores 6/10 → Same row updated
User retakes quiz, scores 9/10 → Same row updated again
❌ Cannot show evolution! Only see last score (9/10), lose history of 6/10 attempt
```

**Required Implementation:**
```
User reads "Quantum Mechanics" → Activity row 1 (Type="Read")
User takes quiz, scores 6/10 → Activity row 2 (Type="Quiz", Score=6)
User retakes quiz, scores 9/10 → Activity row 3 (Type="Quiz", Score=9)
✅ Full history preserved! Can show progression over time
```

### User Story Requirements

**From Specifications (Section 1.3.7, lines 480-484):**
> **Comparaison (si Sujet Suivi):**
> - Gauche : Score de *cette session*
> - Droite : Meilleur score historique (*Personal Best*)
> - *Carte Festive* : Si le score de la session est un nouveau record !

This requires:
1. ✅ Individual session scores (current attempt) → Need separate rows per attempt
2. ✅ Best score across all attempts → Need aggregation from history
3. ✅ Detect when new record is set → Compare current to historical best

**Current schema cannot do this!**

---

## Part 3: APPROVED Hybrid Architecture

### Overview

```
┌─────────────────────────────────────────┐
│  UserActivity Table                     │
│  (Source of truth - all sessions)       │
│                                         │
│  One row per session:                   │
│  - Read session when user reads         │
│  - Quiz session when user takes quiz    │
│  - Another quiz when user retakes       │
└─────────────────────────────────────────┘
                  │
                  │ Aggregated by service layer
                  ▼
┌─────────────────────────────────────────┐
│  TrackedTopic Table                     │
│  (Performance cache - tracked only)     │
│                                         │
│  One row per tracked topic:             │
│  - BestScore, BestScoreDate             │
│  - FirstAttemptDate, LastAttemptDate    │
│  - Total attempts count                 │
└─────────────────────────────────────────┘
```

---

### Table 1: UserActivity (Individual Sessions)

**Purpose:** Source of truth for all user interactions - one row per session

```csharp
namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a single user session on a Wikipedia topic.
/// Each row is one discrete session (Read or Quiz).
/// </summary>
public class UserActivity
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID of the user who performed this session.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wikipedia topic/article title.
    /// </summary>
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Full Wikipedia URL for the article.
    /// </summary>
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of this session: "Read" (article read) or "Quiz" (quiz completed).
    /// Default is "Read" as reading happens before quizzing.
    /// </summary>
    public string Type { get; set; } = "Read";
    
    /// <summary>
    /// When this session occurred.
    /// For Read: when user finished reading (reached bottom or stayed 3+ minutes)
    /// For Quiz: when user submitted quiz answers
    /// </summary>
    public DateTime SessionDate { get; set; } = DateTime.UtcNow;
    
    // ============================================================
    // Quiz-specific Properties (nullable - only for Type = "Quiz")
    // ============================================================
    
    /// <summary>
    /// Score from this quiz session.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public int? Score { get; set; }
    
    /// <summary>
    /// Total number of questions in this quiz session.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public int? TotalQuestions { get; set; }
    
    /// <summary>
    /// Name of the LLM model used to generate this quiz.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public string? LlmModelName { get; set; }
    
    /// <summary>
    /// Version of the LLM model used.
    /// Only set when Type = "Quiz", null for Type = "Read".
    /// </summary>
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User? User { get; set; }
}
```

**Key Changes from Current Model:**
- ✅ **REMOVED** `FirstAttemptDate` (aggregated data → TrackedTopic)
- ✅ **REMOVED** `LastAttemptDate` (aggregated data → TrackedTopic)
- ✅ **REMOVED** `BestScore` (aggregated data → TrackedTopic)
- ✅ **REMOVED** `IsTracked` (replaced by TrackedTopic table existence)
- ✅ **ADDED** `SessionDate` (when this specific session occurred)
- ✅ **RENAMED** `LastScore` → `Score` (this session's score)
- ✅ **MADE NULLABLE** `Score`, `TotalQuestions`
- ✅ **CHANGED DEFAULT** `Type` from `"Quiz"` to `"Read"`

---

### Table 2: TrackedTopic (Aggregated Summary)

**Purpose:** Performance-optimized cache for tracked topics only

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
    
    // ============================================================
    // Cached Aggregated Data (denormalized for performance)
    // Updated whenever a new UserActivity session is created
    // ============================================================
    
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

**Key Features:**
- ✅ Exists only for tracked topics (smaller table = better performance)
- ✅ All aggregated data can be rebuilt from `UserActivity` history
- ✅ `TrackedDate` auto-set when tracking enabled (user cannot change per requirement #4)
- ✅ Updated automatically via service layer when new sessions are created

---

## Part 4: Checking if Topic is Tracked

**USER DECISION:** Remove `IsTracked` from `UserActivity`, rely solely on `TrackedTopic` table.

### How to Check if a Topic is Tracked

**Backend (Service Layer):**
```csharp
public async Task<bool> IsTopicTracked(string userId, string topic)
{
    return await _context.TrackedTopics
        .AnyAsync(t => t.UserId == userId && t.Topic == topic);
}
```

**Database Query (EF Core):**
```csharp
// Get activities with tracked indicator
var activities = await _context.Activities
    .Where(a => a.UserId == userId)
    .Select(a => new {
        a.Id,
        a.Topic,
        a.Type,
        a.SessionDate,
        a.Score,
        // Check if this topic exists in TrackedTopics
        IsTracked = _context.TrackedTopics
            .Any(t => t.UserId == userId && t.Topic == a.Topic)
    })
    .ToListAsync();
```

**Performance Consideration:**
- ✅ `TrackedTopics` table is small (only tracked topics, not all activities)
- ✅ Indexed on `UserId` and `Topic` for fast lookups
- ✅ Join is efficient due to table size

---

## Part 5: Data Flow Examples

### Scenario 1: User Reads an Article (Not Tracked)

```csharp
// User reaches bottom of page or stays 3+ minutes
// Create Read session
var activity = new UserActivity
{
    UserId = "user1",
    Topic = "Quantum Mechanics",
    WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
    Type = "Read",
    SessionDate = DateTime.UtcNow,
    Score = null, // No quiz taken
    TotalQuestions = null,
    LlmModelName = null,
    LlmVersion = null
};
await _activityRepo.CreateAsync(activity);

// Check if topic is tracked
var isTracked = await _trackedRepo.ExistsAsync("user1", "Quantum Mechanics");
if (isTracked)
{
    // Update TrackedTopic aggregated data
    var tracked = await _trackedRepo.GetByTopicAsync("user1", "Quantum Mechanics");
    tracked.TotalReadSessions++;
    tracked.LastReadDate = DateTime.UtcNow;
    if (tracked.FirstReadDate == null)
        tracked.FirstReadDate = DateTime.UtcNow;
    
    await _trackedRepo.UpdateAsync(tracked);
}
// else: Not tracked, no TrackedTopic to update
```

---

### Scenario 2: User Takes Quiz (First Attempt on Tracked Topic)

```csharp
// User submits quiz answers
// Create Quiz session
var activity = new UserActivity
{
    UserId = "user1",
    Topic = "Quantum Mechanics",
    WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
    Type = "Quiz",
    SessionDate = DateTime.UtcNow,
    Score = 6,
    TotalQuestions = 10,
    LlmModelName = "llama3:8b",
    LlmVersion = "1.0"
};
await _activityRepo.CreateAsync(activity);

// Update TrackedTopic (if tracked)
var tracked = await _trackedRepo.GetByTopicAsync("user1", "Quantum Mechanics");
if (tracked != null)
{
    tracked.TotalQuizAttempts++;
    tracked.LastAttemptDate = DateTime.UtcNow;
    
    if (tracked.FirstAttemptDate == null)
        tracked.FirstAttemptDate = DateTime.UtcNow;
    
    // First quiz attempt - set best score
    if (tracked.BestScore == null || 6 > tracked.BestScore)
    {
        tracked.BestScore = 6;
        tracked.TotalQuestions = 10;
        tracked.BestScoreDate = DateTime.UtcNow;
    }
    
    await _trackedRepo.UpdateAsync(tracked);
}
```

---

### Scenario 3: User Retakes Quiz (New Personal Record! 🎉)

```csharp
// User takes quiz again - create NEW session row
var activity = new UserActivity
{
    UserId = "user1",
    Topic = "Quantum Mechanics",
    WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
    Type = "Quiz",
    SessionDate = DateTime.UtcNow,
    Score = 9, // Improved!
    TotalQuestions = 10,
    LlmModelName = "llama3:8b",
    LlmVersion = "1.0"
};
await _activityRepo.CreateAsync(activity);

// Update TrackedTopic
var tracked = await _trackedRepo.GetByTopicAsync("user1", "Quantum Mechanics");
if (tracked != null)
{
    tracked.TotalQuizAttempts++;
    tracked.LastAttemptDate = DateTime.UtcNow;
    
    // Check if new best score
    if (9 > tracked.BestScore) // 🎉 New record!
    {
        tracked.BestScore = 9;
        tracked.TotalQuestions = 10;
        tracked.BestScoreDate = DateTime.UtcNow;
    }
    
    await _trackedRepo.UpdateAsync(tracked);
}

// Frontend can now compare:
// - This session score: 9/10 (from UserActivity)
// - Previous best: 6/10 → Now 9/10 (from TrackedTopic)
// - Show "🎉 New Personal Record!" celebration
```

---

### Scenario 4: User Tracks a Topic

```csharp
// User clicks "Track this topic" ⭐
// Check if already tracked
var existing = await _trackedRepo.GetByTopicAsync(userId, topic);
if (existing != null)
{
    return; // Already tracked
}

// Create TrackedTopic entry
var trackedTopic = new TrackedTopic
{
    UserId = userId,
    Topic = topic,
    WikipediaUrl = wikipediaUrl,
    TrackedDate = DateTime.UtcNow, // Auto-set, user cannot change
    TotalReadSessions = 0,
    TotalQuizAttempts = 0
};

// Rebuild aggregated data from existing UserActivity history
var allSessions = await _activityRepo.GetAllForTopicAsync(userId, topic);

foreach (var session in allSessions)
{
    if (session.Type == "Read")
    {
        trackedTopic.TotalReadSessions++;
        trackedTopic.LastReadDate = session.SessionDate;
        if (trackedTopic.FirstReadDate == null)
            trackedTopic.FirstReadDate = session.SessionDate;
    }
    else if (session.Type == "Quiz")
    {
        trackedTopic.TotalQuizAttempts++;
        trackedTopic.LastAttemptDate = session.SessionDate;
        if (trackedTopic.FirstAttemptDate == null)
            trackedTopic.FirstAttemptDate = session.SessionDate;
        
        if (trackedTopic.BestScore == null || session.Score > trackedTopic.BestScore)
        {
            trackedTopic.BestScore = session.Score;
            trackedTopic.TotalQuestions = session.TotalQuestions;
            trackedTopic.BestScoreDate = session.SessionDate;
        }
    }
}

await _trackedRepo.CreateAsync(trackedTopic);
```

---

### Scenario 5: User Untracks a Topic

```csharp
// User clicks "Untrack" ⭐ (toggle off)
var tracked = await _trackedRepo.GetByTopicAsync(userId, topic);
if (tracked != null)
{
    await _trackedRepo.DeleteAsync(tracked.Id);
}

// UserActivity history is preserved!
// If user re-tracks later, history will be rebuilt
```

---

## Part 6: Database Indexes for Performance

### UserActivity Table Indexes

```csharp
// In DerotDbContext.OnModelCreating()

modelBuilder.Entity<UserActivity>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // Index for user's activity history (most common query)
    entity.HasIndex(e => new { e.UserId, e.SessionDate })
        .HasDatabaseName("IX_UserActivity_UserId_SessionDate");
    
    // Index for topic-specific history (evolution queries)
    entity.HasIndex(e => new { e.UserId, e.Topic, e.SessionDate })
        .HasDatabaseName("IX_UserActivity_UserId_Topic_SessionDate");
    
    // Index for type filtering (Read vs Quiz)
    entity.HasIndex(e => new { e.UserId, e.Type, e.SessionDate })
        .HasDatabaseName("IX_UserActivity_UserId_Type_SessionDate");
    
    // Foreign key
    entity.HasOne(e => e.User)
        .WithMany(u => u.Activities)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

### TrackedTopic Table Indexes

```csharp
modelBuilder.Entity<TrackedTopic>(entity =>
{
    entity.HasKey(e => e.Id);
    
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

---

## Part 7: DTO Changes Required

### CreateActivityDto (for creating new sessions)

```csharp
using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for creating a new user activity session.
/// </summary>
public class CreateActivityDto
{
    [Required]
    public string Topic { get; set; } = string.Empty;
    
    [Required]
    [Url]
    public string WikipediaUrl { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression("^(Read|Quiz)$")]
    public string Type { get; set; } = "Read"; // Changed default to "Read"
    
    // Quiz-specific properties (nullable - only for Type = "Quiz")
    [Range(0, int.MaxValue)]
    public int? Score { get; set; } // Made nullable
    
    [Range(1, int.MaxValue)]
    public int? TotalQuestions { get; set; } // Made nullable
    
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    // Validate that Quiz activities have required fields
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == "Quiz")
        {
            if (!Score.HasValue)
                yield return new ValidationResult(
                    "Score is required for Quiz activities",
                    new[] { nameof(Score) });
            
            if (!TotalQuestions.HasValue)
                yield return new ValidationResult(
                    "TotalQuestions is required for Quiz activities",
                    new[] { nameof(TotalQuestions) });
        }
    }
}
```

### UserActivityDto (for API responses)

```csharp
namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for user activity session details.
/// </summary>
public class UserActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string WikipediaUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; } // NEW: replaces FirstAttemptDate/LastAttemptDate
    
    // Quiz-specific (nullable)
    public int? Score { get; set; } // Renamed from LastScore
    public int? TotalQuestions { get; set; }
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
}
```

### TrackedTopicDto (new DTO)

```csharp
namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for tracked topic summary.
/// </summary>
public class TrackedTopicDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string WikipediaUrl { get; set; } = string.Empty;
    public DateTime TrackedDate { get; set; }
    
    // Aggregated statistics
    public int TotalReadSessions { get; set; }
    public int TotalQuizAttempts { get; set; }
    
    public DateTime? FirstReadDate { get; set; }
    public DateTime? LastReadDate { get; set; }
    public DateTime? FirstAttemptDate { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    
    public int? BestScore { get; set; }
    public int? TotalQuestions { get; set; }
    public DateTime? BestScoreDate { get; set; }
}
```

---

## Part 8: Migration Strategy

**USER DECISION:** Ignore existing `UserActivity` data (development phase)

### Steps

1. **Create `TrackedTopic` entity and DbSet**
   - Add `TrackedTopic.cs` model
   - Add `DbSet<TrackedTopic> TrackedTopics` to `DerotDbContext`

2. **Update `UserActivity` entity**
   - Remove: `FirstAttemptDate`, `LastAttemptDate`, `BestScore`, `IsTracked`
   - Add: `SessionDate` property
   - Rename: `LastScore` → `Score`
   - Make nullable: `Score`, `TotalQuestions`
   - Change default: `Type = "Read"`

3. **Create new EF Core migration**
   ```bash
   dotnet ef migrations add HybridArchitecture_UserActivity_TrackedTopic
   ```

4. **Apply migration (will drop existing data per user decision)**
   ```bash
   dotnet ef database update
   ```

5. **Update DbContext configuration** (indexes as shown in Part 6)

6. **Create repository interfaces and implementations**
   - `ITrackedTopicRepository`
   - `SqliteTrackedTopicRepository`

7. **Update service layer logic** to:
   - Create session records instead of updating aggregates
   - Sync TrackedTopic data when activities are created
   - Handle tracking/untracking operations

---

## Part 9: API Endpoints Required

### UserActivity Endpoints (existing, update DTOs)

- `GET /api/users/{userId}/activities` - Get all sessions (paginated)
- `GET /api/users/{userId}/activities?topic={topic}` - Get sessions for specific topic (evolution)
- `POST /api/users/{userId}/activities` - Create new session
- `DELETE /api/users/{userId}/activities/{id}` - Delete session (if needed)

### TrackedTopic Endpoints (new)

- `GET /api/users/{userId}/tracked-topics` - Get all tracked topics with aggregated data
- `POST /api/users/{userId}/tracked-topics` - Track a topic (creates TrackedTopic + rebuilds history)
- `DELETE /api/users/{userId}/tracked-topics/{topicId}` - Untrack a topic
- `GET /api/users/{userId}/tracked-topics/{topicId}/evolution` - Get evolution/history (joins UserActivity)

---

## Part 10: Testing Requirements

### Unit Tests

**UserActivity Service:**
- ✅ Create Read session
- ✅ Create Quiz session with valid score
- ✅ Validate Quiz session requires score/questions
- ✅ Validate Read session allows null score/questions
- ✅ Prevent duplicate sessions within time window (optional)

**TrackedTopic Service:**
- ✅ Track new topic (creates TrackedTopic)
- ✅ Track existing topic (idempotent)
- ✅ Rebuild aggregated data from UserActivity history
- ✅ Update aggregated data when new session added
- ✅ Detect new best score and update BestScoreDate
- ✅ Untrack topic (deletes TrackedTopic, preserves UserActivity)

### Integration Tests

**Activity History:**
- ✅ Get all sessions for user (ordered by SessionDate desc)
- ✅ Filter sessions by Type (Read vs Quiz)
- ✅ Get sessions for specific topic (evolution view)
- ✅ Include IsTracked indicator in response

**Tracked Topics:**
- ✅ Get tracked topics list with aggregated data
- ✅ Sort by LastAttemptDate, BestScore, TrackedDate
- ✅ Verify aggregated data matches UserActivity history
- ✅ Verify TrackedDate is auto-set and immutable

---

## Summary of Approved Decisions

1. ✅ **APPROVED** Hybrid architecture (UserActivity + TrackedTopic tables)
2. ✅ **MIGRATION** Ignore existing data (development phase - drop and recreate)
3. ✅ **IS_TRACKED** Remove from UserActivity, rely on TrackedTopic table existence
4. ✅ **TRACKED_DATE** Auto-set when tracking enabled, user cannot change

---

## Next Steps for Implementation

See companion prompt: `DerotProperties-Implementation-Prompt.md`

**Implementation order:**
1. Create `TrackedTopic` model and DbContext configuration
2. Update `UserActivity` model (remove/add/rename properties)
3. Generate and apply EF Core migration
4. Create `ITrackedTopicRepository` and implementation
5. Update `IActivityService` with new logic
6. Create DTOs (`TrackedTopicDto`, update `CreateActivityDto`)
7. Create/update API endpoints
8. Write unit and integration tests
9. Update seed data (`DbInitializer.cs`)
10. Test end-to-end flow

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-21  
**Status:** Ready for implementation
