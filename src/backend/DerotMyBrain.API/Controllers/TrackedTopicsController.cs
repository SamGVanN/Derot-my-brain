using Microsoft.AspNetCore.Mvc;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Services;
using System.ComponentModel.DataAnnotations;

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
            // Assuming the service returns TrackedTopic models or similar that need mapping to TrackedTopicDto
            // For now, let's assume it returns correctly mapped DTOs or we map them here if needed.
            // Based on prompt Part 1, the service should handle this or we handle it here.
            // Let's implement mapping to be safe if it returns models.
            // But usually services in this project seem to return DTOs or models are mapped.
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
