using Microsoft.AspNetCore.Mvc;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace DerotMyBrain.API.Controllers;

/// <summary>
/// Controller for managing tracked topics.
/// </summary>
[ApiController]
[Authorize]
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
    public async Task<ActionResult<IEnumerable<TrackedTopic>>> GetTrackedTopics(string userId)
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
    public async Task<ActionResult<TrackedTopic>> GetTrackedTopic(string userId, string topic)
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
    public async Task<ActionResult<TrackedTopic>> TrackTopic(
        string userId,
        [FromBody] TrackTopicRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var trackedTopic = await _trackedTopicService.TrackTopicAsync(
                userId, request.Topic, request.WikipediaUrl);
            
            return CreatedAtAction(
                nameof(GetTrackedTopic),
                new { userId, topic = request.Topic },
                trackedTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking topic {Topic} for user {UserId}", request.Topic, userId);
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
    public async Task<ActionResult<IEnumerable<UserActivity>>> GetTopicEvolution(
        string userId, 
        string topic)
    {
        try
        {
            // Verify topic is tracked (implied logic, or we check)
            var tracked = await _trackedTopicService.GetTrackedTopicAsync(userId, topic);
            if (tracked == null)
                return NotFound(new { message = $"Topic '{topic}' is not tracked" });
            
            // Get all sessions for this topic
            var activities = await _activityService.GetAllForTopicAsync(userId, topic);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evolution for topic {Topic}, user {UserId}", topic, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    public class TrackTopicRequest 
    {
        [Required]
        public string Topic { get; set; } = string.Empty;
        public string WikipediaUrl { get; set; } = string.Empty;
    }
}
