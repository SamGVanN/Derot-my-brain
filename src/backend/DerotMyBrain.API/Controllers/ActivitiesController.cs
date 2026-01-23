using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Route("api/users/{userId}")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet("activities")]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetAllActivities(string userId, [FromQuery] string? topic = null)
    {
        try
        {
            IEnumerable<UserActivity> activities;
            if (!string.IsNullOrEmpty(topic))
            {
                activities = await _activityService.GetAllForTopicAsync(userId, topic);
            }
            else
            {
                activities = await _activityService.GetAllActivitiesAsync(userId);
            }
            
            // Map to DTO (Use AutoMapper in prod, manual for now)
            var dtos = activities.Select(MapToDto).ToList();
            return Ok(dtos);
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
            return Ok(MapToDto(activity));
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
            var resultDto = MapToDto(activity);
            
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
            return Ok(MapToDto(activity));
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

    // --- Legacy / Specific Endpoints ---

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

    private UserActivityDto MapToDto(UserActivity activity)
    {
        return new UserActivityDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            Topic = activity.Title, // Map Title -> Topic dto property
            WikipediaUrl = activity.SourceUrl ?? "",
            Type = activity.Type,
            SessionDate = activity.LastAttemptDate,
            Score = activity.Score == 0 && activity.MaxScore == 0 ? null : activity.Score, // Heuristic for logic, or just return as is?
            // DTO has int? Score. Entity has int Score. Tests expect null for Read. 
            // The activity created for Read usually has 0/0.
            TotalQuestions = activity.MaxScore == 0 ? null : activity.MaxScore,
            LlmModelName = activity.LlmModelName,
            LlmVersion = activity.LlmVersion,
            IsTracked = activity.IsTracked
        };
    }
}
