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
    private readonly ITrackedTopicService _trackedTopicService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(
        IActivityService activityService, 
        ITrackedTopicService trackedTopicService,
        ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _trackedTopicService = trackedTopicService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all activities for a specific user, optionally filtered by topic.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topic">Optional topic filter.</param>
    /// <returns>List of activities.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetActivities(
        string userId,
        [FromQuery] string? topic = null)
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

            // Efficiently fetch all tracked topic names for the user to avoid N+1 queries
            var trackedTopicNames = (await _trackedTopicService.GetAllTrackedTopicsAsync(userId))
                .Select(t => t.Topic)
                .ToHashSet();

            var dtos = activities.Select(a => MapToDto(a, trackedTopicNames.Contains(a.Topic)));
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for user {UserId}, Topic: {Topic}", userId, topic);
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

            var isTracked = await _activityService.IsTopicTrackedAsync(userId, activity.Topic);
            return Ok(MapToDto(activity, isTracked));
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
            var isTracked = await _activityService.IsTopicTrackedAsync(userId, activity.Topic);
            var resultDto = MapToDto(activity, isTracked);
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
            var isTracked = await _activityService.IsTopicTrackedAsync(userId, activity.Topic);
            _logger.LogInformation("Activity updated: {ActivityId} for user {UserId}, Score: {Score}", activityId, userId, activity.Score);
            return Ok(MapToDto(activity, isTracked));
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

    private static UserActivityDto MapToDto(UserActivity activity, bool isTracked = false)
    {
        return new UserActivityDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            Topic = activity.Topic,
            WikipediaUrl = activity.WikipediaUrl,
            Type = activity.Type,
            SessionDate = activity.SessionDate,
            Score = activity.Score,
            TotalQuestions = activity.TotalQuestions,
            LlmModelName = activity.LlmModelName,
            LlmVersion = activity.LlmVersion,
            IsTracked = isTracked
        };
    }
}
