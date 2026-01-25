using Microsoft.AspNetCore.Mvc;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace DerotMyBrain.API.Controllers;

/// <summary>
/// Controller for managing user focal points (UserFocus).
/// </summary>
[ApiController]
[Authorize]
[Route("api/users/{userId}/user-focus")]
public class UserFocusController : ControllerBase
{
    private readonly IUserFocusService _userFocusService;
    private readonly IActivityService _activityService;
    private readonly ILogger<UserFocusController> _logger;
    
    public UserFocusController(
        IUserFocusService userFocusService,
        IActivityService activityService,
        ILogger<UserFocusController> logger)
    {
        _userFocusService = userFocusService;
        _activityService = activityService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all focal points for a user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserFocusDto>>> GetFocuses(string userId)
    {
        try
        {
            var focuses = await _userFocusService.GetAllFocusesAsync(userId);
            return Ok(focuses.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user focus areas for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Gets a specific focal point by hash.
    /// </summary>
    [HttpGet("{sourceHash}")]
    public async Task<ActionResult<UserFocusDto>> GetFocus(string userId, string sourceHash)
    {
        try
        {
            var focus = await _userFocusService.GetFocusAsync(userId, sourceHash);
            
            if (focus == null)
                return NotFound(new { message = $"Topic with hash '{sourceHash}' is not in focus" });
            
            return Ok(MapToDto(focus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user focus {Hash} for user {UserId}", sourceHash, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Tracks a topic (adds to focus area) for the user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserFocusDto>> TrackTopic(
        string userId,
        [FromBody] TrackTopicRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var focus = await _userFocusService.TrackTopicAsync(
                userId, request.SourceId, request.SourceType, request.DisplayTitle);
            
            return CreatedAtAction(
                nameof(GetFocus),
                new { userId, sourceHash = focus.SourceHash },
                MapToDto(focus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding focus {Title} for user {UserId}", request.DisplayTitle, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Untracks a topic (removes from focus area) for the user.
    /// </summary>
    [HttpDelete("{sourceHash}")]
    public async Task<IActionResult> UntrackTopic(string userId, string sourceHash)
    {
        try
        {
            await _userFocusService.UntrackTopicAsync(userId, sourceHash);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error untracking focal point {Hash} for user {UserId}", sourceHash, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
    
    /// <summary>
    /// Gets the evolution/history of a focus area (all sessions).
    /// </summary>
    [HttpGet("{sourceHash}/evolution")]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetFocusEvolution(
        string userId, 
        string sourceHash)
    {
        try
        {
            var focus = await _userFocusService.GetFocusAsync(userId, sourceHash);
            if (focus == null)
                return NotFound(new { message = $"Topic with hash '{sourceHash}' is not in focus" });
            
            return Ok(await _activityService.GetAllForContentAsync(userId, sourceHash));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evolution for focus {Hash}, user {UserId}", sourceHash, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Toggles PIN status.
    /// </summary>
    [HttpPatch("{sourceHash}/pin")]
    public async Task<ActionResult<UserFocusDto>> TogglePin(string userId, string sourceHash)
    {
        try
        {
            var focus = await _userFocusService.TogglePinAsync(userId, sourceHash);
            if (focus == null) return NotFound();
            return Ok(MapToDto(focus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling pin for focus {Hash}, user {UserId}", sourceHash, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Toggles ARCHIVE status.
    /// </summary>
    [HttpPatch("{sourceHash}/archive")]
    public async Task<ActionResult<UserFocusDto>> ToggleArchive(string userId, string sourceHash)
    {
        try
        {
            var focus = await _userFocusService.ToggleArchiveAsync(userId, sourceHash);
            if (focus == null) return NotFound();
            return Ok(MapToDto(focus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling archive for focus {Hash}, user {UserId}", sourceHash, userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private UserFocusDto MapToDto(UserFocus t)
    {
        return new UserFocusDto
        {
            Id = t.Id,
            UserId = t.UserId,
            SourceHash = t.SourceHash,
            SourceId = t.SourceId,
            SourceType = t.SourceType,
            DisplayTitle = t.DisplayTitle,
            BestScore = t.BestScore,
            LastScore = t.LastScore,
            LastAttemptDate = t.LastAttemptDate,
            TotalReadTimeSeconds = t.TotalReadTimeSeconds,
            TotalQuizTimeSeconds = t.TotalQuizTimeSeconds,
            TotalStudyTimeSeconds = t.TotalStudyTimeSeconds,
            IsPinned = t.IsPinned,
            IsArchived = t.IsArchived
        };
    }
    
    public class TrackTopicRequest 
    {
        [Required]
        public string SourceId { get; set; } = string.Empty;
        
        [Required]
        public SourceType SourceType { get; set; }

        [Required]
        public string DisplayTitle { get; set; } = string.Empty;
    }
}
