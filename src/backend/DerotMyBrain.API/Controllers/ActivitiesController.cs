using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

/// <summary>
/// Controller for managing user activities, tracked topics, and statistics.
/// </summary>
[ApiController]
[Route("api/users/{userId}/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all activities for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of activities.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetActivities(string userId)
    {
        try
        {
            var activities = await _activityService.GetAllActivitiesAsync(userId);
            return Ok(activities.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets a specific activity by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <returns>The activity details.</returns>
    [HttpGet("{activityId}")]
    public async Task<ActionResult<UserActivityDto>> GetActivity(string userId, string activityId)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(userId, activityId);
            if (activity == null)
            {
                _logger.LogWarning("Activity {ActivityId} not found for user {UserId}", activityId, userId);
                return NotFound(new { message = $"Activity {activityId} not found" });
            }
            return Ok(MapToDto(activity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity {ActivityId} for user {UserId}", activityId, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="dto">The activity creation data.</param>
    /// <returns>The created activity.</returns>
    [HttpPost]
    public async Task<ActionResult<UserActivityDto>> CreateActivity(string userId, [FromBody] CreateActivityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var activity = await _activityService.CreateActivityAsync(userId, dto);
            var resultDto = MapToDto(activity);
            _logger.LogInformation("Activity created: {ActivityId} for user {UserId}, Topic: {Topic}", activity.Id, userId, activity.Topic);
            return CreatedAtAction(nameof(GetActivity), new { userId, activityId = activity.Id }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>The updated activity.</returns>
    [HttpPut("{activityId}")]
    public async Task<ActionResult<UserActivityDto>> UpdateActivity(string userId, string activityId, [FromBody] UpdateActivityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var activity = await _activityService.UpdateActivityAsync(userId, activityId, dto);
            _logger.LogInformation("Activity updated: {ActivityId} for user {UserId}, LastScore: {LastScore}", activityId, userId, activity.LastScore);
            return Ok(MapToDto(activity));
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Activity {ActivityId} not found for update, user {UserId}", activityId, userId);
            return NotFound(new { message = $"Activity {activityId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {ActivityId} for user {UserId}", activityId, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Deletes an activity.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{activityId}")]
    public async Task<IActionResult> DeleteActivity(string userId, string activityId)
    {
        try
        {
            await _activityService.DeleteActivityAsync(userId, activityId);
            _logger.LogInformation("Activity deleted: {ActivityId} for user {UserId}", activityId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Activity {ActivityId} not found for deletion, user {UserId}", activityId, userId);
            return NotFound(new { message = $"Activity {activityId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {ActivityId} for user {UserId}", activityId, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // --- Tracking Endpoints ---

    /// <summary>
    /// Gets all tracked topics (favorites) for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of tracked activities.</returns>
    [HttpGet("~/api/users/{userId}/tracked-topics")]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetTrackedTopics(string userId)
    {
        try
        {
            var activities = await _activityService.GetTrackedActivitiesAsync(userId);
            return Ok(activities.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracked topics for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Adds a topic to tracked topics (favorites).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <returns>No content.</returns>
    [HttpPost("{activityId}/track")]
    public async Task<IActionResult> TrackActivity(string userId, string activityId)
    {
        try
        {
            await _activityService.TrackActivityAsync(userId, activityId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking activity {ActivityId} for user {UserId}", activityId, userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Removes a topic from tracked topics.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{activityId}/track")]
    public async Task<IActionResult> UntrackActivity(string userId, string activityId)
    {
        try
        {
            await _activityService.UntrackActivityAsync(userId, activityId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error untracking activity {ActivityId} for user {UserId}", activityId, userId);
            return StatusCode(500, "Internal server error");
        }
    }

    // --- Dashboard Endpoints ---

    /// <summary>
    /// Gets global statistics for the user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>User statistics.</returns>
    [HttpGet("~/api/users/{userId}/statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics(string userId)
    {
        try
        {
            var stats = await _activityService.GetStatisticsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets activity calendar data for heatmaps.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="days">Number of days to include (default 365).</param>
    /// <returns>List of calendar data points.</returns>
    [HttpGet("~/api/users/{userId}/statistics/activity-calendar")]
    public async Task<ActionResult<IEnumerable<ActivityCalendarDto>>> GetActivityCalendar(string userId, [FromQuery] int days = 365)
    {
        try
        {
            var calendar = await _activityService.GetActivityCalendarAsync(userId, days);
            return Ok(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity calendar for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets top scores for the user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="limit">Number of top scores to return.</param>
    /// <returns>List of top scores.</returns>
    [HttpGet("~/api/users/{userId}/statistics/top-scores")]
    public async Task<ActionResult<IEnumerable<TopScoreDto>>> GetTopScores(string userId, [FromQuery] int limit = 10)
    {
        try
        {
            var scores = await _activityService.GetTopScoresAsync(userId, limit);
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top scores for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    private static UserActivityDto MapToDto(UserActivity activity)
    {
        return new UserActivityDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            Topic = activity.Topic,
            WikipediaUrl = activity.WikipediaUrl,
            FirstAttemptDate = activity.FirstAttemptDate,
            LastAttemptDate = activity.LastAttemptDate,
            LastScore = activity.LastScore,
            BestScore = activity.BestScore,
            TotalQuestions = activity.TotalQuestions,
            LlmModelName = activity.LlmModelName,
            LlmVersion = activity.LlmVersion,
            IsTracked = activity.IsTracked,
            Type = activity.Type
        };
    }
}
