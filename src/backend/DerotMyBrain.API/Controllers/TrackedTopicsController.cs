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
    [HttpGet("{title}")]
    public async Task<ActionResult<TrackedTopic>> GetTrackedTopic(string userId, string title)
    {
        try
        {
            var trackedTopic = await _trackedTopicService.GetTrackedTopicAsync(userId, title);
            
            if (trackedTopic == null)
                return NotFound(new { message = $"Topic '{title}' is not tracked" });
            
            return Ok(trackedTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracked topic {Title} for user {UserId}", title, userId);
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
                userId, request.Title, request.WikipediaUrl);
            
            return CreatedAtAction(
                nameof(GetTrackedTopic),
                new { userId, title = request.Title },
                trackedTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking topic {Title} for user {UserId}", request.Title, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Untracks a topic for the user.
    /// </summary>
    [HttpDelete("{title}")]
    public async Task<IActionResult> UntrackTopic(string userId, string title)
    {
        try
        {
            await _trackedTopicService.UntrackTopicAsync(userId, title);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error untracking topic {Title} for user {UserId}", title, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Gets the evolution/history of a tracked topic (all sessions).
    /// </summary>
    [HttpGet("{title}/evolution")]
    public async Task<ActionResult<IEnumerable<UserActivity>>> GetTopicEvolution(
        string userId, 
        string title)
    {
        try
        {
            // Verify topic is tracked (implied logic, or we check)
            var tracked = await _trackedTopicService.GetTrackedTopicAsync(userId, title);
            if (tracked == null)
                return NotFound(new { message = $"Topic '{title}' is not tracked" });
            
            // Get all sessions for this topic
            var activities = await _activityService.GetAllForTopicAsync(userId, title);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evolution for topic {Title}, user {UserId}", title, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    public class TrackTopicRequest 
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string WikipediaUrl { get; set; } = string.Empty;
    }
}
