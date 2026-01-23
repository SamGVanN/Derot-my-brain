using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<DerotMyBrain.Core.DTOs.ContentResult>> StartReading([FromQuery] string userId, [FromBody] StartActivityRequest request)
    {
        // Ideally ID comes from Auth token, but using query param as per existing pattern or simplicity
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

    [HttpPost("{activityId}/quiz")]
    public async Task<ActionResult<QuizDto>> GenerateQuiz(string activityId, [FromQuery] string userId)
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

    // --- Existing Dashboard Endpoints (Delegating to Service) ---

    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics([FromQuery] string userId)
    {
        return Ok(await _activityService.GetStatisticsAsync(userId));
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<IEnumerable<ActivityCalendarDto>>> GetCalendar([FromQuery] string userId, [FromQuery] int days = 365)
    {
        return Ok(await _activityService.GetActivityCalendarAsync(userId, days));
    }

    [HttpGet("top-scores")]
    public async Task<ActionResult<IEnumerable<TopScoreDto>>> GetTopScores([FromQuery] string userId, [FromQuery] int limit = 10)
    {
        return Ok(await _activityService.GetTopScoresAsync(userId, limit));
    }
}
