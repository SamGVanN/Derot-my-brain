# Implementation Part 2: API Layer (Phases 5-7)

**Status:** 🟡 Requires Part 1 completion first  
**Estimated Time:** 20-30 minutes  
**Prerequisites:** Part 1 complete (models, database, repositories, services)  
**Next:** [`DerotProperties-Prompt-Part3.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties-Prompt-Part3.md) (Testing & Validation)

---

## Overview

This is **Part 2 of 3** in the UserActivity property refactoring. You will implement the API layer:
- Update DTOs to match new model schema
- Update ActivitiesController
- Create TrackedTopicsController
- Configure dependency injection

**Prerequisites:** Part 1 must be complete and tested.

**Read first:** [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for full context.

---

## Phase 5: DTOs

### 5.1 Update CreateActivityDto

**File**: `src/backend/DerotMyBrain.API/DTOs/CreateActivityDto.cs`

**Changes Required:**
- Change `Type` default to `"Read"`
- Make `Score` nullable (int → int?)
- Make `TotalQuestions` nullable (int → int?)
- Add custom validation (Quiz requires Score/TotalQuestions)

**Implementation:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for creating a new user activity session.
/// </summary>
public class CreateActivityDto : IValidatableObject
{
    [Required]
    public string Topic { get; set; } = string.Empty;
    
    [Required]
    [Url]
    public string WikipediaUrl { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression("^(Read|Quiz)$", ErrorMessage = "Type must be 'Read' or 'Quiz'")]
    public string Type { get; set; } = "Read"; // Changed from "Quiz"
    
    // Quiz-specific properties (nullable)
    [Range(0, int.MaxValue, ErrorMessage = "Score must be non-negative")]
    public int? Score { get; set; } // Made nullable
    
    [Range(1, int.MaxValue, ErrorMessage = "TotalQuestions must be at least 1")]
    public int? TotalQuestions { get; set; } // Made nullable
    
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
    
    /// <summary>
    /// Custom validation: Quiz activities must have Score and TotalQuestions.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == "Quiz")
        {
            if (!Score.HasValue)
            {
                yield return new ValidationResult(
                    "Score is required for Quiz activities",
                    new[] { nameof(Score) });
            }
            
            if (!TotalQuestions.HasValue)
            {
                yield return new ValidationResult(
                    "TotalQuestions is required for Quiz activities",
                    new[] { nameof(TotalQuestions) });
            }
        }
    }
}
```

**Checklist:**
- [x] `Type` default changed to `"Read"`
- [x] `Score` made nullable (int?)
- [x] `TotalQuestions` made nullable (int?)
- [x] Custom validation implemented
- [x] DTO implements IValidatableObject

---

### 5.2 Update UserActivityDto

**File:** `src/backend/DerotMyBrain.API/DTOs/UserActivityDto.cs`

**Changes Required:**
- **REMOVE**: `FirstAttemptDate`, `LastAttemptDate`, `BestScore`, `IsTracked`
- **ADD**: `SessionDate`
- **RENAME**: `LastScore` → `Score`
- **MAKE NULLABLE**: `Score`, `TotalQuestions`

**Implementation:**
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
    
    /// <summary>
    /// When this session occurred.
    /// </summary>
    public DateTime SessionDate { get; set; } // NEW (replaces FirstAttemptDate/LastAttemptDate)
    
    // Quiz-specific (nullable)
    public int? Score { get; set; } // Renamed from LastScore, nullable
    public int? TotalQuestions { get; set; } // Nullable
    public string? LlmModelName { get; set; }
    public string? LlmVersion { get; set; }
}
```

**Checklist:**
- [x] `FirstAttemptDate` removed
- [x] `LastAttemptDate` removed
- [x] `BestScore` removed
- [x] `IsTracked` removed
- [x] `SessionDate` added
- [x] `LastScore` renamed to `Score`
- [x] `Score` and `TotalQuestions` nullable

---

### 5.3 Create TrackedTopicDto

**File:** `src/backend/DerotMyBrain.API/DTOs/TrackedTopicDto.cs` (NEW)

**Implementation:**
```csharp
namespace DerotMyBrain.API.DTOs;

/// <summary>
/// DTO for tracked topic summary with aggregated statistics.
/// </summary>
public class TrackedTopicDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string WikipediaUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// When the user first tracked this topic.
    /// </summary>
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

**Checklist:**
- [x] File created
- [x] All properties implemented
- [x] XML documentation added

---

### 5.4 Update UpdateActivityDto (if exists)

**File:** `src/backend/DerotMyBrain.API/DTOs/UpdateActivityDto.cs`

**Changes:**
- Make `Score` nullable (int → int?)
- Make `TotalQuestions` nullable (int → int?)

**Checklist:**
- [x] Properties made nullable (if file exists)

---

## Phase 6: Controllers

### 6.1 Update ActivitiesController

**File:** `src/backend/DerotMyBrain.API/Controllers/ActivitiesController.cs`

**Changes Required:**

1. **Update mapping logic** (LastScore → Score, add SessionDate)
2. **Add IsTracked indicator** in responses
3. **Add endpoint** for topic evolution

**Key Updates:**

**Update GET all activities endpoint:**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetActivities(
    [FromQuery] string userId,
    [FromQuery] string? topic = null,
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 20)
{
    try
    {
        IEnumerable<UserActivity> activities;
        
        if (!string.IsNullOrEmpty(topic))
        {
            // Get evolution for specific topic
            activities = await _activityService.GetAllForTopicAsync(userId, topic);
        }
        else
        {
            // Get all activities (paginated)
            activities = await _activityService.GetAllAsync(userId);
        }
        
        var activityDtos = activities
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(a => new UserActivityDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Topic = a.Topic,
                WikipediaUrl = a.WikipediaUrl,
                Type = a.Type,
                SessionDate = a.SessionDate, // UPDATED
                Score = a.Score, // UPDATED (was LastScore)
                TotalQuestions = a.TotalQuestions,
                LlmModelName = a.LlmModelName,
                LlmVersion = a.LlmVersion
            });
        
        return Ok(activityDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
        return StatusCode(500, new { message = "Internal server error" });
    }
}
```

**Update POST create activity endpoint:**
```csharp
[HttpPost]
public async Task<ActionResult<UserActivityDto>> CreateActivity(
    [FromQuery] string userId,
    [FromBody] CreateActivityDto dto)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var activity = await _activityService.CreateActivityAsync(userId, dto);
        
        // Map to DTO
        var activityDto = new UserActivityDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            Topic = activity.Topic,
            WikipediaUrl = activity.WikipediaUrl,
            Type = activity.Type,
            SessionDate = activity.SessionDate, // UPDATED
            Score = activity.Score, // UPDATED
            TotalQuestions = activity.TotalQuestions,
            LlmModelName = activity.LlmModelName,
            LlmVersion = activity.LlmVersion
        };
        
        return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activityDto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating activity for user {UserId}", userId);
        return StatusCode(500, new { message = "Internal server error" });
    }
}
```

**Checklist:**
- [x] All mapping updated (LastScore → Score, SessionDate added)
- [x] GET endpoint supports `?topic=X` query for evolution
- [x] POST endpoint validates dto properly
- [x] No references to removed properties (FirstAttemptDate, BestScore, IsTracked)

---

### 6.2 Create TrackedTopicsController

**File:** `src/backend/DerotMyBrain.API/Controllers/TrackedTopicsController.cs` (NEW)

**Implementation:**
```csharp
using Microsoft.AspNetCore.Mvc;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Services;

namespace DerotMyBrain.API.Controllers;

/// <summary>
/// Controller for managing tracked topics.
/// </summary>
[ApiController]
[Route("api/users/{userId}/tracked-topics")]
public class TrackedTopicsController : ControllerBase
{
    private readonly ITrackedTopicService _trackedTopicService;
    private readonly IActivityService _activityService;
    private readonly ILogger<TrackedTopicsController> _logger;
    
    public TrackedTopicsController(
        ITrackedTopicService trackedTopicService,
        IActivityService activityService,
        ILogger<TrackedTopicsController> logger)
    {
        _trackedTopicService = trackedTopicService;
        _activityService = activityService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all tracked topics for a user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrackedTopicDto>>> GetTrackedTopics(string userId)
    {
        try
        {
            var trackedTopics = await _trackedTopicService.GetAllTrackedTopicsAsync(userId);
            return Ok(trackedTopics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracked topics for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Gets a specific tracked topic.
    /// </summary>
    [HttpGet("{topic}")]
    public async Task<ActionResult<TrackedTopicDto>> GetTrackedTopic(string userId, string topic)
    {
        try
        {
            var trackedTopic = await _trackedTopicService.GetTrackedTopicAsync(userId, topic);
            
            if (trackedTopic == null)
                return NotFound(new { message = $"Topic '{topic}' is not tracked" });
            
            return Ok(trackedTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracked topic {Topic} for user {UserId}", topic, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Tracks a topic for the user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TrackedTopicDto>> TrackTopic(
        string userId,
        [FromBody] TrackTopicDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var trackedTopic = await _trackedTopicService.TrackTopicAsync(
                userId, dto.Topic, dto.WikipediaUrl);
            
            return CreatedAtAction(
                nameof(GetTrackedTopic),
                new { userId, topic = dto.Topic },
                trackedTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking topic {Topic} for user {UserId}", dto.Topic, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Untracks a topic for the user.
    /// </summary>
    [HttpDelete("{topic}")]
    public async Task<IActionResult> UntrackTopic(string userId, string topic)
    {
        try
        {
            await _trackedTopicService.UntrackTopicAsync(userId, topic);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error untracking topic {Topic} for user {UserId}", topic, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Gets the evolution/history of a tracked topic (all sessions).
    /// </summary>
    [HttpGet("{topic}/evolution")]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetTopicEvolution(
        string userId, 
        string topic)
    {
        try
        {
            // Verify topic is tracked
            var isTracked = await _activityService.IsTopicTrackedAsync(userId, topic);
            if (!isTracked)
                return NotFound(new { message = $"Topic '{topic}' is not tracked" });
            
            // Get all sessions for this topic
            var activities = await _activityService.GetAllForTopicAsync(userId, topic);
            
            var activityDtos = activities.Select(a => new UserActivityDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Topic = a.Topic,
                WikipediaUrl = a.WikipediaUrl,
                Type = a.Type,
                SessionDate = a.SessionDate,
                Score = a.Score,
                TotalQuestions = a.TotalQuestions,
                LlmModelName = a.LlmModelName,
                LlmVersion = a.LlmVersion
            });
            
            return Ok(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evolution for topic {Topic}, user {UserId}", topic, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

/// <summary>
/// DTO for tracking a topic.
/// </summary>
public class TrackTopicDto
{
    [Required]
    public string Topic { get; set; } = string.Empty;
    
    [Required]
    [Url]
    public string WikipediaUrl { get; set; } = string.Empty;
}
```

**Checklist:**
- [x] Controller created
- [x] GET all tracked topics endpoint
- [x] GET specific tracked topic endpoint
- [x] POST track topic endpoint
- [x] DELETE untrack topic endpoint
- [x] GET topic evolution endpoint
- [x] Structured logging on all endpoints
- [x] Proper error handling (try-catch)
- [x] TrackTopicDto defined

---

## Phase 7: Dependency Injection

### 7.1 Update Program.cs

**File:** `src/backend/DerotMyBrain.API/Program.cs`

**Add service registrations:**
```csharp
// Repositories
builder.Services.AddScoped<IActivityRepository, SqliteActivityRepository>();
builder.Services.AddScoped<ITrackedTopicRepository, SqliteTrackedTopicRepository>(); // NEW

// Services
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ITrackedTopicService, TrackedTopicService>(); // NEW
```

**Checklist:**
- [x] ITrackedTopicRepository and SqliteTrackedTopicRepository registered
- [x] ITrackedTopicService and TrackedTopicService registered
- [x] Both use `Scoped` lifetime

---

## Exit Criteria - Part 2 Complete ✅

Before proceeding to Part 3, verify:

### Code Compilation
- [x] Solution builds without errors
- [x] No compilation warnings

### DTOs
- [x] CreateActivityDto updated (Type default, nullable fields, validation)
- [x] UserActivityDto updated (SessionDate, Score, no removed properties)
- [x] TrackedTopicDto created
- [x] UpdateActivityDto updated (if exists)

### Controllers
- [x] ActivitiesController updated (mapping, endpoints)
- [x] TrackedTopicsController created with all 5 endpoints
- [x] All endpoints have proper error handling

### Dependency Injection
- [x] All new services registered in Program.cs

### Quick API Test (Optional)
Test endpoints manually or with Swagger:

**Create Read session:**
```http
POST /api/users/{userId}/activities
{
  "topic": "Quantum Mechanics",
  "wikipediaUrl": "https://...",
  "type": "Read"
}
```

**Create Quiz session:**
```http
POST /api/users/{userId}/activities
{
  "topic": "Quantum Mechanics",
  "wikipediaUrl": "https://...",
  "type": "Quiz",
  "score": 8,
  "totalQuestions": 10
}
```

**Track topic:**
```http
POST /api/users/{userId}/tracked-topics
{
  "topic": "Quantum Mechanics",
  "wikipediaUrl": "https://..."
}
```

**Get tracked topics:**
```http
GET /api/users/{userId}/tracked-topics
```

---

## Next Steps

Once all exit criteria are met:

**👉 Proceed to:** [`DerotProperties-Prompt-Part3.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties-Prompt-Part3.md)

Part 3 will implement:
- Seed data (demonstrating session evolution)
- Unit tests
- Integration tests
- Final validation

---

**Need help?** Reference [`DerotProperties.md`](file:///d:/Repos/Derot-my-brain/Docs/Planning/DerotProperties.md) for detailed specifications.
