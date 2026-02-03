using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId}/sources")]
public class SourcesController : ControllerBase
{
    private readonly ISourceService _sourceService;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(ISourceService sourceService, ILogger<SourcesController> logger)
    {
        _sourceService = sourceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrackedSourceDto>>> GetSources([FromRoute] string userId, [FromQuery] bool? tracked = null)
    {
        try
        {
            if (tracked == true)
            {
                var sources = await _sourceService.GetTrackedSourcesAsync(userId);
                return Ok(sources);
            }
            // If tracked is null or false, we might want to return all sources or just searched ones.
            // For now, the requirement is mainly for tracked sources.
            return Ok(new List<TrackedSourceDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sources for user {UserId}", userId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("{sourceId}")]
    public async Task<ActionResult<Source>> GetSource([FromRoute] string userId, [FromRoute] string sourceId)
    {
        try
        {
            var source = await _sourceService.GetSourceAsync(sourceId);
            if (source == null) return NotFound();
            
            // Basic security check
            if (!string.IsNullOrEmpty(source.UserId) && source.UserId != userId)
            {
                return Forbid();
            }

            return Ok(source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting source {SourceId}", sourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TrackedSourceDto>> TrackSource([FromRoute] string userId, [FromBody] TrackTopicRequest request)
    {
        try
        {
            var source = await _sourceService.TrackSourceAsync(userId, request.SourceId, request.DisplayTitle, request.SourceType);
            
            // Fetch DTO to return to frontend
            var dto = await _sourceService.GetTrackedSourceAsync(userId, source.Id);
            
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking source {SourceId}", request.SourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPatch("{sourceId}/track")]
    public async Task<ActionResult<Source>> ToggleTracking([FromRoute] string userId, [FromRoute] string sourceId, [FromBody] ToggleTrackingRequest request)
    {
        try
        {
            var source = await _sourceService.ToggleTrackingAsync(userId, sourceId, request.IsTracked);
            return Ok(source);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Source not found");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling tracking for source {SourceId}", sourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPatch("/api/users/{userId}/user-focus/{sourceId}/pin")]
    public async Task<ActionResult<TrackedSourceDto>> TogglePin([FromRoute] string userId, [FromRoute] string sourceId)
    {
        try
        {
            var source = await _sourceService.TogglePinAsync(userId, sourceId);
            var dto = await _sourceService.GetTrackedSourceAsync(userId, source.Id);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Source not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling pin for source {SourceId}", sourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPatch("/api/users/{userId}/user-focus/{sourceId}/archive")]
    public async Task<ActionResult<TrackedSourceDto>> ToggleArchive([FromRoute] string userId, [FromRoute] string sourceId)
    {
        try
        {
            var source = await _sourceService.ToggleArchiveAsync(userId, sourceId);
            var dto = await _sourceService.GetTrackedSourceAsync(userId, source.Id);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Source not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling archive for source {SourceId}", sourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("{sourceId}/extraction-status")]
    public async Task<ActionResult<ContentExtractionStatusDto>> GetExtractionStatus(
        [FromRoute] string userId, 
        [FromRoute] string sourceId)
    {
        try
        {
            var source = await _sourceService.GetSourceAsync(sourceId);
            if (source == null || source.UserId != userId)
            {
                return NotFound("Source not found");
            }

            return Ok(new ContentExtractionStatusDto
            {
                SourceId = source.Id,
                Status = source.ContentExtractionStatus,
                Error = source.ContentExtractionError,
                CompletedAt = source.ContentExtractionCompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting extraction status for source {SourceId}", sourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    public class TrackTopicRequest
    {
        public required string SourceId { get; set; }
        public SourceType SourceType { get; set; }
        public required string DisplayTitle { get; set; }
    }

    public class ToggleTrackingRequest
    {
        public bool IsTracked { get; set; }
    }

    public class ContentExtractionStatusDto
    {
        public required string SourceId { get; set; }
        public ContentExtractionStatus Status { get; set; }
        public string? Error { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
