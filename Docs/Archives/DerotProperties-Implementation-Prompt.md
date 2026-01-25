# Implementation Prompt: Fix UserActivity Property Inconsistencies with Hybrid Architecture

## Context

You are implementing a **critical architectural fix** for the UserActivity model in the Derot My Brain application. The current schema does not align with functional specifications and has property inconsistencies that prevent proper session tracking.

**Read the full analysis first:** [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md)

---

## Your Task

Refactor the `UserActivity` model and introduce a new `TrackedTopic` model to support:
1. ✅ Individual session tracking (one row per Read/Quiz session)
2. ✅ History evolution for the same topic across multiple attempts
3. ✅ Performance-optimized queries for Tracked Topics page
4. ✅ Nullable properties for Read activities (no quiz = no score/dates)

---

## Required Reading

**Before you start, review these documents:**

1. **[DerotProperties.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md)** - Full architecture analysis and approved solution
2. **[functional_specifications_derot_my_brain.md](file:///d:/Repos/Derot-my-brain/Docs/Planning/functional_specifications_derot_my_brain.md)** - Functional requirements (especially section 1.3.7)
3. **[Backend-Guidelines.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Backend-Guidelines.md)** - Coding standards and patterns
4. **[Testing-Strategy.md](file:///d:/Repos/Derot-my-brain/Docs/Technical/Testing-Strategy.md)** - TDD requirements

---

## Approved Architecture

**Hybrid approach with two tables:**

```
UserActivity Table          TrackedTopic Table
├─ One row per session      ├─ One row per tracked topic
├─ Source of truth          ├─ Aggregated cache
├─ Read sessions            ├─ BestScore, dates
├─ Quiz sessions            ├─ Total attempts
└─ Individual scores        └─ Performance optimized
```

**Key principle:** `UserActivity` stores individual sessions. `TrackedTopic` caches aggregated data for tracked topics only.

---

## Implementation Checklist

### Phase 1: Model Changes

#### 1.1 Create TrackedTopic Model
- [ ] Create `src/backend/DerotMyBrain.API/Models/TrackedTopic.cs`
- [ ] Implement properties as specified in `DerotProperties.md` Part 3, Table 2
- [ ] Add XML documentation for all properties
- [ ] Set `TrackedDate` default to `DateTime.UtcNow`

**Template:**
```csharp
namespace DerotMyBrain.API.Models;

/// <summary>
/// Represents a topic that the user is tracking for mastery.
/// This is a denormalized cache table for performance.
/// All data can be rebuilt from UserActivity history.
/// </summary>
public class TrackedTopic
{
    // See DerotProperties.md Part 3, Table 2 for full implementation
}
```

#### 1.2 Update UserActivity Model
- [ ] **REMOVE** properties: `FirstAttemptDate`, `LastAttemptDate`, `BestScore`, `IsTracked`
- [ ] **ADD** property: `SessionDate` (DateTime, default `DateTime.UtcNow`)
- [ ] **RENAME** property: `LastScore` → `Score`
- [ ] **MAKE NULLABLE**: `Score` (int → int?), `TotalQuestions` (int → int?)
- [ ] **CHANGE DEFAULT**: `Type` from `"Quiz"` to `"Read"`
- [ ] Update XML documentation accordingly

**Reference:** `DerotProperties.md` Part 3, Table 1

#### 1.3 Update User Model
- [ ] Add navigation property: `public List<TrackedTopic> TrackedTopics { get; set; } = new();`

### Phase 2: Database Changes

#### 2.1 Update DerotDbContext
- [ ] Add `DbSet<TrackedTopic> TrackedTopics { get; set; }`
- [ ] Configure `UserActivity` entity (remove old indexes, add new ones per Part 6)
- [ ] Configure `TrackedTopic` entity (indexes, unique constraint on UserId+Topic)
- [ ] Set up foreign key relationships

**Reference:** `DerotProperties.md` Part 6

#### 2.2 Generate EF Core Migration
- [ ] Run: `dotnet ef migrations add HybridArchitecture_UserActivity_TrackedTopic`
- [ ] Review generated migration (verify drop/create operations)
- [ ] Apply migration: `dotnet ef database update`

**Note:** Existing data will be dropped per user approval (development phase).

### Phase 3: Repository Layer

#### 3.1 Create ITrackedTopicRepository
- [ ] Create `src/backend/DerotMyBrain.API/Repositories/ITrackedTopicRepository.cs`
- [ ] Define methods:
  - `Task<TrackedTopic?> GetByIdAsync(string id)`
  - `Task<TrackedTopic?> GetByTopicAsync(string userId, string topic)`
  - `Task<IEnumerable<TrackedTopic>> GetAllAsync(string userId)`
  - `Task<TrackedTopic> CreateAsync(TrackedTopic trackedTopic)`
  - `Task UpdateAsync(TrackedTopic trackedTopic)`
  - `Task DeleteAsync(string id)`
  - `Task<bool> ExistsAsync(string userId, string topic)`

#### 3.2 Create SqliteTrackedTopicRepository
- [ ] Create `src/backend/DerotMyBrain.API/Repositories/SqliteTrackedTopicRepository.cs`
- [ ] Implement `ITrackedTopicRepository` interface
- [ ] Follow patterns from existing `SqliteActivityRepository`
- [ ] Add structured logging for all operations
- [ ] Handle exceptions appropriately

**Reference:** `Backend-Guidelines.md` Section 2 (Repository Pattern)

#### 3.3 Update IActivityRepository
- [ ] Add method: `Task<IEnumerable<UserActivity>> GetAllForTopicAsync(string userId, string topic)`
- [ ] Update existing methods if DTOs changed

#### 3.4 Update SqliteActivityRepository
- [ ] Implement new `GetAllForTopicAsync` method
- [ ] Update queries to use `SessionDate` instead of `LastAttemptDate`
- [ ] Remove any references to `IsTracked` property

### Phase 4: Service Layer

#### 4.1 Update IActivityService
- [ ] Update method signatures to match new DTOs
- [ ] Add method: `Task<bool> IsTopicTrackedAsync(string userId, string topic)`

#### 4.2 Update ActivityService
- [ ] Inject `ITrackedTopicRepository` via constructor
- [ ] When creating activity:
  - Create `UserActivity` session record
  - If topic is tracked, update `TrackedTopic` aggregated data (see Part 5, Scenarios 1-3)
- [ ] Implement `IsTopicTrackedAsync` (check `TrackedTopics` table)
- [ ] Add logging for all operations

#### 4.3 Create ITrackedTopicService
- [ ] Create `src/backend/DerotMyBrain.API/Services/ITrackedTopicService.cs`
- [ ] Define methods:
  - `Task<TrackedTopicDto> TrackTopicAsync(string userId, string topic, string wikipediaUrl)`
  - `Task UntrackTopicAsync(string userId, string topic)`
  - `Task<IEnumerable<TrackedTopicDto>> GetAllTrackedTopicsAsync(string userId)`
  - `Task<TrackedTopicDto?> GetTrackedTopicAsync(string userId, string topic)`

#### 4.4 Create TrackedTopicService
- [ ] Create `src/backend/DerotMyBrain.API/Services/TrackedTopicService.cs`
- [ ] Implement `ITrackedTopicService` interface
- [ ] **TrackTopic logic:**
  - Check if already tracked (idempotent)
  - Create `TrackedTopic` entry with `TrackedDate = DateTime.UtcNow`
  - Rebuild aggregated data from `UserActivity` history (see Part 5, Scenario 4)
- [ ] **UntrackTopic logic:**
  - Delete `TrackedTopic` entry
  - Preserve `UserActivity` history
- [ ] Add structured logging
- [ ] Follow service layer patterns from `Backend-Guidelines.md`

### Phase 5: DTOs

#### 5.1 Update CreateActivityDto
- [ ] Change `Type` default to `"Read"`
- [ ] Make `Score` nullable (int → int?)
- [ ] Make `TotalQuestions` nullable (int → int?)
- [ ] Add custom validation: If `Type == "Quiz"`, require `Score` and `TotalQuestions`

**Reference:** `DerotProperties.md` Part 7

#### 5.2 Update UserActivityDto
- [ ] **REMOVE**: `FirstAttemptDate`, `LastAttemptDate`, `BestScore`, `IsTracked`
- [ ] **ADD**: `SessionDate` (DateTime)
- [ ] **RENAME**: `LastScore` → `Score`
- [ ] **MAKE NULLABLE**: `Score`, `TotalQuestions`

#### 5.3 Create TrackedTopicDto
- [ ] Create `src/backend/DerotMyBrain.API/DTOs/TrackedTopicDto.cs`
- [ ] Implement properties as specified in Part 7

#### 5.4 Update UpdateActivityDto (if applicable)
- [ ] Make `Score` and `TotalQuestions` nullable

### Phase 6: Controllers

#### 6.1 Update ActivitiesController
- [ ] Update endpoints to use new DTOs
- [ ] Update mapping logic (e.g., `LastScore` → `Score`, add `SessionDate`)
- [ ] Add endpoint: `GET /api/users/{userId}/activities?topic={topic}` for evolution view
- [ ] Add `IsTracked` indicator in responses (join with `TrackedTopics`)

#### 6.2 Create TrackedTopicsController
- [ ] Create `src/backend/DerotMyBrain.API/Controllers/TrackedTopicsController.cs`
- [ ] Implement endpoints:
  - `GET /api/users/{userId}/tracked-topics` - List tracked topics
  - `POST /api/users/{userId}/tracked-topics` - Track a topic
  - `DELETE /api/users/{userId}/tracked-topics/{topicId}` - Untrack a topic
  - `GET /api/users/{userId}/tracked-topics/{topicId}/evolution` - Get session history for topic
- [ ] Follow controller patterns from `Backend-Guidelines.md` Section 4
- [ ] Add structured logging
- [ ] Return appropriate HTTP status codes

### Phase 7: Dependency Injection

#### 7.1 Update Program.cs
- [ ] Register `ITrackedTopicRepository` and `SqliteTrackedTopicRepository`
- [ ] Register `ITrackedTopicService` and `TrackedTopicService`
- [ ] Use `Scoped` lifetime (per request)

**Example:**
```csharp
builder.Services.AddScoped<ITrackedTopicRepository, SqliteTrackedTopicRepository>();
builder.Services.AddScoped<ITrackedTopicService, TrackedTopicService>();
```

### Phase 8: Seed Data

#### 8.1 Update DbInitializer.cs
- [ ] Remove references to `IsTracked`, `FirstAttemptDate`, `LastAttemptDate`, `BestScore`
- [ ] Update test data to use `SessionDate` instead
- [ ] Create multiple `UserActivity` sessions for same topic (show evolution)
- [ ] Create corresponding `TrackedTopic` entries for tracked topics
- [ ] Demonstrate:
  - Read session → Quiz session on same topic
  - Multiple quiz attempts showing score improvement
  - Tracked vs non-tracked topics

**Example:**
```csharp
// Session 1: Read
new UserActivity { 
    Topic = "Quantum", 
    Type = "Read", 
    SessionDate = DateTime.UtcNow.AddDays(-5),
    Score = null 
}

// Session 2: First quiz
new UserActivity { 
    Topic = "Quantum", 
    Type = "Quiz", 
    SessionDate = DateTime.UtcNow.AddDays(-3),
    Score = 6,
    TotalQuestions = 10
}

// Session 3: Retry quiz (improved!)
new UserActivity { 
    Topic = "Quantum", 
    Type = "Quiz", 
    SessionDate = DateTime.UtcNow,
    Score = 9,
    TotalQuestions = 10
}

// TrackedTopic: Aggregated
new TrackedTopic {
    Topic = "Quantum",
    TrackedDate = DateTime.UtcNow.AddDays(-4),
    TotalReadSessions = 1,
    TotalQuizAttempts = 2,
    BestScore = 9,
    BestScoreDate = DateTime.UtcNow
}
```

### Phase 9: Testing (TDD)

#### 9.1 Unit Tests - ActivityService
- [ ] Test: Create Read session (Score/TotalQuestions are null)
- [ ] Test: Create Quiz session (Score/TotalQuestions required)
- [ ] Test: Update TrackedTopic when session created on tracked topic
- [ ] Test: Do NOT update TrackedTopic when session created on non-tracked topic
- [ ] Test: Detect new best score and update BestScoreDate

#### 9.2 Unit Tests - TrackedTopicService
- [ ] Test: Track new topic (creates TrackedTopic)
- [ ] Test: Track already-tracked topic (idempotent)
- [ ] Test: Rebuild aggregated data from UserActivity history
- [ ] Test: Untrack topic (deletes TrackedTopic, preserves UserActivity)
- [ ] Test: TrackedDate is auto-set and cannot be changed

#### 9.3 Integration Tests - ActivityController
- [ ] Test: GET all activities (paginated, ordered by SessionDate)
- [ ] Test: GET activities for specific topic (evolution view)
- [ ] Test: POST new Read session
- [ ] Test: POST new Quiz session (validates Score/TotalQuestions)
- [ ] Test: Response includes IsTracked indicator

#### 9.4 Integration Tests - TrackedTopicController
- [ ] Test: GET tracked topics list
- [ ] Test: POST track topic (creates TrackedTopic + rebuilds history)
- [ ] Test: DELETE untrack topic
- [ ] Test: GET topic evolution (joins UserActivity sessions)

**Reference:** `Testing-Strategy.md` and `Backend-Guidelines.md` Section 6

### Phase 10: Validation & Cleanup

#### 10.1 Manual Testing
- [ ] Test Read session creation (no score)
- [ ] Test Quiz session creation (with score)
- [ ] Test multiple quiz attempts on same topic
- [ ] Test tracking/untracking topics
- [ ] Test "new personal record" scenario (BestScore update)
- [ ] Verify TrackedDate is immutable

#### 10.2 Code Review Checklist
- [ ] All properties match `DerotProperties.md` specifications
- [ ] No hardcoded values (use `DateTime.UtcNow` for dates)
- [ ] Structured logging on all operations
- [ ] Proper error handling (try-catch, custom exceptions)
- [ ] XML documentation on public APIs
- [ ] Follow naming conventions (PascalCase, camelCase)
- [ ] No `.Result` or `.Wait()` (async/await only)

#### 10.3 Database Verification
- [ ] Run migrations successfully
- [ ] Verify indexes created (check with DB tool)
- [ ] Verify unique constraint on TrackedTopic (UserId, Topic)
- [ ] Verify foreign keys and cascade delete

---

## Key Implementation Notes

### 1. TrackedDate Behavior (USER DECISION #4)
```csharp
// ✅ CORRECT: Auto-set, user cannot change
var trackedTopic = new TrackedTopic 
{ 
    TrackedDate = DateTime.UtcNow // Auto-set
};

// ❌ WRONG: Do NOT allow user to set TrackedDate
// CreateTrackedTopicDto should NOT have TrackedDate property
```

### 2. Checking if Topic is Tracked (USER DECISION #3)
```csharp
// ✅ CORRECT: Check TrackedTopics table
var isTracked = await _context.TrackedTopics
    .AnyAsync(t => t.UserId == userId && t.Topic == topic);

// ❌ WRONG: Do NOT use IsTracked property (removed from UserActivity)
```

### 3. Session vs Aggregation
```csharp
// ✅ CORRECT: UserActivity stores individual sessions
var session = new UserActivity 
{ 
    Type = "Quiz", 
    Score = 9, 
    SessionDate = DateTime.UtcNow 
};

// ✅ CORRECT: TrackedTopic stores aggregated best score
var tracked = await GetTrackedTopic(...);
if (session.Score > tracked.BestScore)
{
    tracked.BestScore = session.Score;
    tracked.BestScoreDate = DateTime.UtcNow;
}
```

### 4. Data Synchronization Pattern
```csharp
// When creating a new UserActivity session:
public async Task<UserActivity> CreateActivityAsync(CreateActivityDto dto)
{
    // 1. Create session record (source of truth)
    var activity = new UserActivity { ... };
    await _activityRepo.CreateAsync(activity);
    
    // 2. Update TrackedTopic cache (if tracked)
    if (await IsTopicTrackedAsync(activity.UserId, activity.Topic))
    {
        var tracked = await _trackedRepo.GetByTopicAsync(...);
        // Update aggregated fields (TotalAttempts, BestScore, etc.)
        await _trackedRepo.UpdateAsync(tracked);
    }
    
    return activity;
}
```

---

## Expected Deliverables

1. ✅ Updated `UserActivity` model with nullable properties
2. ✅ New `TrackedTopic` model with aggregated data
3. ✅ EF Core migration applied successfully
4. ✅ Repository layer (ITrackedTopicRepository + implementation)
5. ✅ Service layer (TrackedTopicService + updated ActivityService)
6. ✅ DTOs (TrackedTopicDto + updated Create/UserActivityDto)
7. ✅ Controllers (TrackedTopicsController + updated ActivitiesController)
8. ✅ Unit tests (≥80% coverage)
9. ✅ Integration tests (all endpoints)
10. ✅ Updated seed data demonstrating session evolution

---

## Testing the Implementation

### Manual Test Scenario

**Story:** User reads an article, takes quiz twice, tracks topic

1. **Create Read session**
   ```
   POST /api/users/{userId}/activities
   { "topic": "Quantum Mechanics", "type": "Read" }
   → Should create UserActivity with Score=null, SessionDate=now
   ```

2. **Create first Quiz session**
   ```
   POST /api/users/{userId}/activities
   { "topic": "Quantum Mechanics", "type": "Quiz", "score": 6, "totalQuestions": 10 }
   → Should create UserActivity with Score=6
   ```

3. **Track the topic**
   ```
   POST /api/users/{userId}/tracked-topics
   { "topic": "Quantum Mechanics" }
   → Should create TrackedTopic
   → Should rebuild history: TotalReadSessions=1, TotalQuizAttempts=1, BestScore=6
   ```

4. **Create second Quiz session (improvement!)**
   ```
   POST /api/users/{userId}/activities
   { "topic": "Quantum Mechanics", "type": "Quiz", "score": 9, "totalQuestions": 10 }
   → Should create UserActivity with Score=9
   → Should update TrackedTopic: TotalQuizAttempts=2, BestScore=9, BestScoreDate=now
   ```

5. **Verify evolution**
   ```
   GET /api/users/{userId}/activities?topic=Quantum%20Mechanics
   → Should return 3 sessions (1 Read + 2 Quiz) ordered by SessionDate
   → Session 1: Type=Read, Score=null
   → Session 2: Type=Quiz, Score=6
   → Session 3: Type=Quiz, Score=9
   ```

6. **Verify tracked topic summary**
   ```
   GET /api/users/{userId}/tracked-topics
   → Should return TrackedTopic with:
     - BestScore=9
     - TotalQuizAttempts=2
     - TotalReadSessions=1
   ```

---

## Common Pitfalls to Avoid

❌ **DON'T** update the same `UserActivity` row when user retakes quiz  
✅ **DO** create a new `UserActivity` row for each quiz attempt

❌ **DON'T** store `IsTracked` in `UserActivity`  
✅ **DO** check if topic exists in `TrackedTopics` table

❌ **DON'T** allow user to set `TrackedDate`  
✅ **DO** auto-set `TrackedDate = DateTime.UtcNow` when tracking

❌ **DON'T** use `.Result` or `.Wait()` for async operations  
✅ **DO** use `async`/`await` throughout

❌ **DON'T** forget to update `TrackedTopic` when new session created  
✅ **DO** sync aggregated data in service layer after session creation

---

## Questions for User (if any arise during implementation)

If you encounter any ambiguities:
1. **Reference `DerotProperties.md` first** - most decisions are documented
2. **Check user decisions** - Migration strategy, IsTracked, TrackedDate behavior
3. **Follow backend guidelines** - Stick to established patterns
4. **Ask specific questions** - If truly blocked, ask user for clarification

---

## Success Criteria

Your implementation is complete when:
- ✅ All tests pass (unit + integration)
- ✅ Code coverage ≥ 80%
- ✅ Manual test scenario works end-to-end
- ✅ Database schema matches `DerotProperties.md` specifications
- ✅ Seed data demonstrates session evolution
- ✅ No linting errors or warnings
- ✅ All methods have XML documentation
- ✅ Structured logging on all operations

---

**Good luck! Refer to [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for detailed specifications and examples.**
