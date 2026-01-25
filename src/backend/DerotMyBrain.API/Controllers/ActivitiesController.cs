using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId}")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly IUserFocusService _userFocusService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(
        IActivityService activityService, 
        IUserFocusService userFocusService,
        ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _userFocusService = userFocusService;
        _logger = logger;
    }

    [HttpGet("activities")]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetAllActivities(string userId, [FromQuery] string? sourceHash = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(sourceHash))
            {
                return Ok(await _activityService.GetAllForContentAsync(userId, sourceHash));
            }
            
            return Ok(await _activityService.GetAllActivitiesAsync(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("activities/{activityId}")]
    public async Task<ActionResult<UserActivityDto>> GetActivity(string userId, string activityId)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(userId, activityId);
            if (activity == null) return NotFound();
            
            var isTracked = await _userFocusService.GetFocusAsync(userId, activity.SourceHash) != null;
            return Ok(MapToDto(activity, isTracked));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity {ActivityId}", activityId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("activities")]
    public async Task<ActionResult<UserActivityDto>> CreateActivity(string userId, [FromBody] CreateActivityDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var activity = await _activityService.CreateActivityAsync(userId, dto);
            var isTracked = await _userFocusService.GetFocusAsync(userId, activity.SourceHash) != null;
            var resultDto = MapToDto(activity, isTracked);
            
            return CreatedAtAction(nameof(GetActivity), new { userId, activityId = activity.Id }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPut("activities/{activityId}")]
    public async Task<ActionResult<UserActivityDto>> UpdateActivity(string userId, string activityId, [FromBody] UpdateActivityDto dto)
    {
        try
        {
            var activity = await _activityService.UpdateActivityAsync(userId, activityId, dto);
            var isTracked = await _userFocusService.GetFocusAsync(userId, activity.SourceHash) != null;
            return Ok(MapToDto(activity, isTracked));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {ActivityId}", activityId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpDelete("activities/{activityId}")]
    public async Task<IActionResult> DeleteActivity(string userId, string activityId)
    {
        try
        {
            await _activityService.DeleteActivityAsync(userId, activityId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {ActivityId}", activityId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    // --- Statistics Endpoints ---

    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics(string userId)
    {
        return Ok(await _activityService.GetStatisticsAsync(userId));
    }

    [HttpGet("statistics/activity-calendar")]
    public async Task<ActionResult<IEnumerable<ActivityCalendarDto>>> GetCalendar(string userId, [FromQuery] int days = 365)
    {
        return Ok(await _activityService.GetActivityCalendarAsync(userId, days));
    }

    [HttpGet("statistics/top-scores")]
    public async Task<ActionResult<IEnumerable<TopScoreDto>>> GetTopScores(string userId, [FromQuery] int limit = 10)
    {
        return Ok(await _activityService.GetTopScoresAsync(userId, limit));
    }

    // --- Content Flow Endpoints ---

    [HttpPost("activities/start")]
    public async Task<ActionResult<DerotMyBrain.Core.DTOs.ContentResult>> StartReading(string userId, [FromBody] StartActivityRequest request)
    {
        try
        {
            var result = await _activityService.StartReadingAsync(userId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting activity");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("activities/{activityId}/quiz")]
    public async Task<ActionResult<QuizDto>> GenerateQuiz(string userId, string activityId)
    {
        try
        {
            var quiz = await _activityService.GenerateQuizAsync(userId, activityId);
            return Ok(quiz);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Activity not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz");
            return StatusCode(500, "Internal Server Error");
        }
    }

    private UserActivityDto MapToDto(UserActivity a, bool isTracked)
    {
        return new UserActivityDto
        {
            Id = a.Id,
            UserId = a.UserId,
            Title = a.Title,
            Description = a.Description,
            SourceId = a.SourceId,
            SourceType = a.SourceType,
            SourceHash = a.SourceHash,
            Type = a.Type,
            SessionDateStart = a.SessionDateStart,
            SessionDateEnd = a.SessionDateEnd,
            ReadDurationSeconds = a.ReadDurationSeconds,
            QuizDurationSeconds = a.QuizDurationSeconds,
            TotalDurationSeconds = a.TotalDurationSeconds,
            Score = a.Score,
            QuestionCount = a.QuestionCount,
            ScorePercentage = a.ScorePercentage,
            IsNewBestScore = a.IsNewBestScore,
            IsCompleted = a.IsCompleted,
            LlmModelName = a.LlmModelName,
            LlmVersion = a.LlmVersion,
            IsTracked = isTracked,
            Payload = a.Payload
        };
    }
}
