# Implementation Part 3: Testing & Validation (Phases 8-10)

**Status:** 🟡 Requires Parts 1 & 2 completion first  
**Estimated Time:** 30-40 minutes  
**Prerequisites:** Parts 1 & 2 complete (core architecture + API layer)  
**Final Phase:** Production-ready implementation ✅

---

## Overview

This is **Part 3 of 3** (final) in the UserActivity property refactoring. You will implement:
- Seed data demonstrating session evolution
- Unit tests for services
- Integration tests for controllers
- Final validation and cleanup

**Prerequisites:** Parts 1 and 2 must be complete and tested.

**Read first:** [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for full context.

---

## Phase 8: Seed Data

### 8.1 Update DbInitializer.cs

**File:** `src/backend/DerotMyBrain.API/Data/DbInitializer.cs`

**Goals:**
- Remove references to removed properties
- Update test data to use `SessionDate` instead of `LastAttemptDate`
- Create multiple `UserActivity` sessions for same topic (demonstrate evolution)
- Create corresponding `TrackedTopic` entries for tracked topics

**Example Seed Data:**

```csharp
public static async Task SeedAsync(DerotDbContext context)
{
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    // Skip if already seeded
    if (context.Users.Any())
        return;
    
    // Create test user
    var testUser = new User
    {
        Id = "test-user-1",
        Name = "TestUser",
        CreatedAt = DateTime.UtcNow.AddMonths(-6)
    };
    context.Users.Add(testUser);
    
    // ============================================
    // Scenario 1: Topic evolution (Quantum Mechanics)
    // ============================================
    
    // Session 1: Read (5 days ago)
    var readSession1 = new UserActivity
    {
        UserId = testUser.Id,
        Topic = "Quantum Mechanics",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
        Type = "Read",
        SessionDate = DateTime.UtcNow.AddDays(-5),
        Score = null, // Read session
        TotalQuestions = null
    };
    context.Activities.Add(readSession1);
    
    // Session 2: First quiz attempt (3 days ago, scored 6/10)
    var quizSession1 = new UserActivity
    {
        UserId = testUser.Id,
        Topic = "Quantum Mechanics",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
        Type = "Quiz",
        SessionDate = DateTime.UtcNow.AddDays(-3),
        Score = 6,
        TotalQuestions = 10,
        LlmModelName = "llama3:8b",
        LlmVersion = "1.0"
    };
    context.Activities.Add(quizSession1);
    
    // Session 3: Second quiz attempt (today, scored 9/10 - improvement!)
    var quizSession2 = new UserActivity
    {
        UserId = testUser.Id,
        Topic = "Quantum Mechanics",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
        Type = "Quiz",
        SessionDate = DateTime.UtcNow,
        Score = 9,
        TotalQuestions = 10,
        LlmModelName = "llama3:8b",
        LlmVersion = "1.0"
    };
    context.Activities.Add(quizSession2);
    
    // TrackedTopic for Quantum Mechanics (tracked 4 days ago)
    var trackedQM = new TrackedTopic
    {
        UserId = testUser.Id,
        Topic = "Quantum Mechanics",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Quantum_mechanics",
        TrackedDate = DateTime.UtcNow.AddDays(-4),
        TotalReadSessions = 1,
        TotalQuizAttempts = 2,
        FirstReadDate = DateTime.UtcNow.AddDays(-5),
        LastReadDate = DateTime.UtcNow.AddDays(-5),
        FirstAttemptDate = DateTime.UtcNow.AddDays(-3),
        LastAttemptDate = DateTime.UtcNow,
        BestScore = 9, // Best score from session 3
        TotalQuestions = 10,
        BestScoreDate = DateTime.UtcNow
    };
    context.TrackedTopics.Add(trackedQM);
    
    // ============================================
    // Scenario 2: Non-tracked topic (Relativity)
    // ============================================
    
    var readSession2 = new UserActivity
    {
        UserId = testUser.Id,
        Topic = "Theory of Relativity",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
        Type = "Read",
        SessionDate = DateTime.UtcNow.AddDays(-2),
        Score = null,
        TotalQuestions = null
    };
    context.Activities.Add(readSession2);
    
    var quizSession3 = new UserActivity
    {
        UserId = testUser.Id,
        Topic = "Theory of Relativity",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Theory_of_relativity",
        Type = "Quiz",
        SessionDate = DateTime.UtcNow.AddDays(-1),
        Score = 7,
        TotalQuestions = 10,
        LlmModelName = "llama3:8b",
        LlmVersion = "1.0"
    };
    context.Activities.Add(quizSession3);
    
    // Note: Relativity is NOT tracked (no TrackedTopic entry)
    
    // ============================================
    // Scenario 3: Tracked topic with only reads (no quiz yet)
    // ============================================
    
    var readSession3 = new UserActivity
    {
        UserId = testUser.Id,
        Topic = "Artificial Intelligence",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
        Type = "Read",
        SessionDate = DateTime.UtcNow.AddDays(-7),
        Score = null,
        TotalQuestions = null
    };
    context.Activities.Add(readSession3);
    
    var trackedAI = new TrackedTopic
    {
        UserId = testUser.Id,
        Topic = "Artificial Intelligence",
        WikipediaUrl = "https://en.wikipedia.org/wiki/Artificial_intelligence",
        TrackedDate = DateTime.UtcNow.AddDays(-7),
        TotalReadSessions = 1,
        TotalQuizAttempts = 0,
        FirstReadDate = DateTime.UtcNow.AddDays(-7),
        LastReadDate = DateTime.UtcNow.AddDays(-7),
        FirstAttemptDate = null, // Never attempted quiz
        LastAttemptDate = null,
        BestScore = null,
        TotalQuestions = null,
        BestScoreDate = null
    };
    context.TrackedTopics.Add(trackedAI);
    
    await context.SaveChangesAsync();
}
```

**Checklist:**
- [ ] All references to removed properties deleted
- [ ] Multiple sessions created for same topic (evolution)
- [ ] TrackedTopic entries created with aggregated data
- [ ] Demonstrates tracked vs non-tracked topics
- [ ] Demonstrates Read-only tracked topic (no quiz yet)
- [ ] Demonstrates best score improvement scenario

---

## Phase 9: Unit Tests

### 9.1 ActivityService Tests

**File:** `src/backend/DerotMyBrain.Tests/Services/ActivityServiceTests.cs`

**Key Tests:**

```csharp
using Xunit;
using Moq;
using DerotMyBrain.API.Services;
using DerotMyBrain.API.Repositories;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<ITrackedTopicRepository> _trackedTopicRepoMock;
    private readonly Mock<ILogger<ActivityService>> _loggerMock;
    private readonly ActivityService _service;
    
    public ActivityServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _trackedTopicRepoMock = new Mock<ITrackedTopicRepository>();
        _loggerMock = new Mock<ILogger<ActivityService>>();
        
        _service = new ActivityService(
            _activityRepoMock.Object,
            _trackedTopicRepoMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateActivityAsync_ReadSession_ShouldHaveNullScore()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Topic = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Read",
            Score = null,
            TotalQuestions = null
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await _service.CreateActivityAsync("user1", dto);
        
        // Assert
        Assert.Equal("Read", result.Type);
        Assert.Null(result.Score);
        Assert.Null(result.TotalQuestions);
        Assert.NotEqual(default(DateTime), result.SessionDate);
    }
    
    [Fact]
    public async Task CreateActivityAsync_QuizSession_ShouldHaveScore()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Topic = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Quiz",
            Score = 8,
            TotalQuestions = 10
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await _service.CreateActivityAsync("user1", dto);
        
        // Assert
        Assert.Equal("Quiz", result.Type);
        Assert.Equal(8, result.Score);
        Assert.Equal(10, result.TotalQuestions);
    }
    
    [Fact]
    public async Task CreateActivityAsync_TrackedTopic_ShouldUpdateCache()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Topic = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Quiz",
            Score = 9,
            TotalQuestions = 10
        };
        
        var trackedTopic = new TrackedTopic
        {
            Id = "tracked1",
            UserId = "user1",
            Topic = "Test",
            BestScore = 6,
            TotalQuizAttempts = 1
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync("user1", "Test"))
            .ReturnsAsync(true);
        
        _trackedTopicRepoMock.Setup(r => r.GetByTopicAsync("user1", "Test"))
            .ReturnsAsync(trackedTopic);
        
        // Act
        var result = await _service.CreateActivityAsync("user1", dto);
        
        // Assert - TrackedTopic should be updated
        _trackedTopicRepoMock.Verify(r => r.UpdateAsync(It.Is<TrackedTopic>(t =>
            t.BestScore == 9 && // New best score
            t.TotalQuizAttempts == 2
        )), Times.Once);
    }
}
```

**Required Tests:**
- [ ] Create Read session (Score/TotalQuestions null)
- [ ] Create Quiz session (Score/TotalQuestions required)
- [ ] Update TrackedTopic when session created on tracked topic
- [ ] Do NOT update TrackedTopic when session created on non-tracked topic
- [ ] Detect new best score and update BestScoreDate

**Checklist:**
- [ ] All tests written and passing
- [ ] Code coverage ≥ 80% for ActivityService

---

### 9.2 TrackedTopicService Tests

**File:** `src/backend/DerotMyBrain.Tests/Services/TrackedTopicServiceTests.cs`

**Required Tests:**
- [ ] Track new topic (creates TrackedTopic)
- [ ] Track already-tracked topic (idempotent, returns existing)
- [ ] Rebuild aggregateddata from UserActivity history
- [ ] Untrack topic (deletes TrackedTopic, preserves UserActivity)
- [ ] TrackedDate is auto-set and cannot be changed

**Checklist:**
- [ ] All tests written and passing
- [ ] Code coverage ≥ 80% for TrackedTopicService

---

## Phase 10: Integration Tests

### 10.1 ActivitiesController Integration Tests

**File:** `src/backend/DerotMyBrain.Tests/Controllers/ActivitiesControllerTests.cs`

**Required Tests:**
- [ ] GET all activities (paginated, ordered by SessionDate desc)
- [ ] GET activities for specific topic (evolution view)
- [ ] POST new Read session
- [ ] POST new Quiz session (validates Score/TotalQuestions required)
- [ ] POST Quiz without score returns 400 Bad Request

**Checklist:**
- [ ] All tests written and passing
- [ ] Uses WebApplicationFactory (if applicable)

---

### 10.2 TrackedTopicsController Integration Tests

**File:** `src/backend/DerotMyBrain.Tests/Controllers/TrackedTopicsControllerTests.cs`

**Required Tests:**
- [ ] GET tracked topics list
- [ ] POST track topic (creates TrackedTopic + rebuilds history)
- [ ] DELETE untrack topic
- [ ] GET topic evolution (returns all sessions for tracked topic)
- [ ] GET topic evolution for non-tracked topic returns 404

**Checklist:**
- [ ] All tests written and passing
- [ ] Uses WebApplicationFactory (if applicable)

---

## Phase 10: Validation & Cleanup

### 10.1 Manual Testing Scenario

Perform end-to-end manual test:

**1. Create Read session:**
```http
POST /api/users/test-user-1/activities
Content-Type: application/json

{
  "topic": "Quantum Computing",
  "wikipediaUrl": "https://en.wikipedia.org/wiki/Quantum_computing",
  "type": "Read"
}

Expected: 201 Created, Score and TotalQuestions are null
```

**2. Create first Quiz session:**
```http
POST /api/users/test-user-1/activities
Content-Type: application/json

{
  "topic": "Quantum Computing",
  "wikipediaUrl": "https://en.wikipedia.org/wiki/Quantum_computing",
  "type": "Quiz",
  "score": 6,
  "totalQuestions": 10
}

Expected: 201 Created, Score=6
```

**3. Track the topic:**
```http
POST /api/users/test-user-1/tracked-topics
Content-Type: application/json

{
  "topic": "Quantum Computing",
  "wikipediaUrl": "https://en.wikipedia.org/wiki/Quantum_computing"
}

Expected: 201 Created, TrackedTopic with:
- TotalReadSessions = 1
- TotalQuizAttempts = 1
- BestScore = 6
```

**4. Create second Quiz session (improvement!):**
```http
POST /api/users/test-user-1/activities
Content-Type: application/json

{
  "topic": "Quantum Computing",
  "wikipediaUrl": "https://en.wikipedia.org/wiki/Quantum_computing",
  "type": "Quiz",
  "score": 9,
  "totalQuestions": 10
}

Expected: 201 Created
```

**5. Verify TrackedTopic updated:**
```http
GET /api/users/test-user-1/tracked-topics

Expected: BestScore = 9, TotalQuizAttempts = 2
```

**6. Verify evolution:**
```http
GET /api/users/test-user-1/activities?topic=Quantum Computing

Expected: 3 sessions ordered by SessionDate:
1. Type=Read, Score=null
2. Type=Quiz, Score=6
3. Type=Quiz, Score=9
```

**Checklist:**
- [ ] All 6 steps complete successfully
- [ ] Read session has null scores
- [ ] Quiz sessions have valid scores
- [ ] TrackedTopic aggregates data correctly
- [ ] BestScore updates on improvement
- [ ] Evolution shows all sessions

---

### 10.2 Code Review Checklist

- [ ] All properties match `DerotProperties.md` specifications
- [ ] No hardcoded values (use `DateTime.UtcNow`)
- [ ] Structured logging on all operations
- [ ] Proper error handling (try-catch, custom exceptions)
- [ ] XML documentation on public APIs
- [ ] Follow naming conventions (PascalCase, camelCase)
- [ ] No `.Result` or `.Wait()` (async/await only)
- [ ] All tests pass (unit + integration)
- [ ] Code coverage ≥ 80%

---

### 10.3 Database Verification

- [ ] Migrations applied successfully
- [ ] `UserActivity` table:
  - [ ] Has `SessionDate` column
  - [ ] Does NOT have `FirstAttemptDate`, `LastAttemptDate`, `BestScore`, `IsTracked`
  - [ ] `Score` and `TotalQuestions` are nullable
  - [ ] Indexes created correctly
- [ ] `TrackedTopic` table:
  - [ ] Exists with all required columns
  - [ ] Unique constraint on (UserId, Topic)
  - [ ] Foreign key to Users table
  - [ ] Indexes created correctly

---

## Final Deliverables ✅

Upon completion, you should have:

1. ✅ **Core Architecture** (Part 1)
   - Updated `UserActivity` model
   - New `TrackedTopic` model
   - Database migration applied
   - Repository layer implemented
   - Service layer implemented

2. ✅ **API Layer** (Part 2)
   - DTOs updated
   - ActivitiesController updated
   - TrackedTopicsController created
   - Dependency injection configured

3. ✅ **Testing & Validation** (Part 3)
   - Seed data demonstrating evolution
   - Unit tests (≥80% coverage)
   - Integration tests
   - Manual testing completed
   - All validations passed

---

## Success Criteria

Your implementation is **COMPLETE** when:

- ✅ All tests pass (unit + integration)
- ✅ Code coverage ≥ 80%
- ✅ Manual test scenario works end-to-end
- ✅ Database schema matches specifications
- ✅ Seed data demonstrates session evolution
- ✅ No compilation errors or warnings
- ✅ All methods have XML documentation
- ✅ Structured logging on all operations
- ✅ Production-ready code quality

---

**🎉 Congratulations!** You've successfully refactored the UserActivity architecture to support individual session tracking and performance-optimized tracked topics.

**Need help?** Reference [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for detailed specifications.
