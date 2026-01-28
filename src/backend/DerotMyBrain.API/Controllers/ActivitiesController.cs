using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId}/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(
        IActivityService activityService, 
        ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetAllActivities([FromRoute] string userId, [FromQuery] string? sourceId = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(sourceId))
            {
                return Ok(await _activityService.GetAllForContentAsync(userId, sourceId));
            }
            
            return Ok(await _activityService.GetAllActivitiesAsync(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("{activityId}")]
    public async Task<ActionResult<UserActivityDto>> GetActivity([FromRoute] string userId, [FromRoute] string activityId)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(userId, activityId);
            if (activity == null) return NotFound();
            
            return Ok(MapToDto(activity, activity.Source?.IsTracked ?? false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity {ActivityId}", activityId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserActivityDto>> CreateActivity([FromRoute] string userId, [FromBody] CreateActivityDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var activity = await _activityService.CreateActivityAsync(userId, dto);
            var resultDto = MapToDto(activity, activity.Source?.IsTracked ?? false);
            
            return CreatedAtAction(nameof(GetActivity), new { userId, activityId = activity.Id }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPut("{activityId}")]
    public async Task<ActionResult<UserActivityDto>> UpdateActivity([FromRoute] string userId, [FromRoute] string activityId, [FromBody] UpdateActivityDto dto)
    {
        try
        {
            var activity = await _activityService.UpdateActivityAsync(userId, activityId, dto);
            return Ok(MapToDto(activity, activity.Source?.IsTracked ?? false));
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

    [HttpDelete("{activityId}")]
    public async Task<IActionResult> DeleteActivity([FromRoute] string userId, [FromRoute] string activityId)
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

    [HttpGet("/api/users/{userId}/user-focus/{sourceId}/evolution")]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetFocusEvolution([FromRoute] string userId, [FromRoute] string sourceId)
    {
        try
        {
            var activities = await _activityService.GetAllForContentAsync(userId, sourceId);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evolution for user {UserId} and source {SourceId}", userId, sourceId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("/api/users/{userId}/statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics([FromRoute] string userId)
    {
        return Ok(await _activityService.GetStatisticsAsync(userId));
    }

    [HttpGet("/api/users/{userId}/statistics/activity-calendar")]
    public async Task<ActionResult<IEnumerable<ActivityCalendarDto>>> GetCalendar([FromRoute] string userId, [FromQuery] int days = 365)
    {
        return Ok(await _activityService.GetActivityCalendarAsync(userId, days));
    }

    [HttpGet("/api/users/{userId}/statistics/top-scores")]
    public async Task<ActionResult<IEnumerable<TopScoreDto>>> GetTopScores([FromRoute] string userId, [FromQuery] int limit = 10)
    {
        return Ok(await _activityService.GetTopScoresAsync(userId, limit));
    }

    [HttpGet("explore/articles")]
    public async Task<ActionResult<IEnumerable<WikipediaArticleDto>>> GetExploreArticles([FromRoute] string userId, [FromQuery] int count = 6)
    {
        try
        {
            var articles = await _activityService.GetExploreArticlesAsync(userId, count);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting explore articles for user {UserId}", userId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("explore")]
    public async Task<ActionResult<UserActivityDto>> Explore([FromRoute] string userId, [FromBody] ExploreRequest req)
    {
        try
        {
            var activity = await _activityService.ExploreAsync(userId, req.Title, req.SourceId, req.SourceType, req.SessionId);
            return Ok(MapToDto(activity, activity.Source?.IsTracked ?? false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting exploration activity");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("read")]
    public async Task<ActionResult<UserActivityDto>> Read([FromRoute] string userId, [FromBody] ReadRequest req)
    {
        try
        {
            var activity = await _activityService.ReadAsync(
                userId, 
                req.Title ?? string.Empty, 
                req.Language, 
                req.SourceId, 
                req.SourceType,
                req.OriginExploreId, 
                req.BacklogAddsCount,
                req.RefreshCount,
                req.ExploreDurationSeconds);

            return Ok(MapToDto(activity, activity.Source?.IsTracked ?? false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting read activity");
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("{activityId}/stop-explore")]
    public async Task<IActionResult> StopExplore([FromRoute] string userId, [FromRoute] string activityId, [FromBody] StopExploreRequest req)
    {
        try
        {
            var updates = new UpdateActivityDto
            {
                DurationSeconds = req.DurationSeconds,
                BacklogAddsCount = req.BacklogAddsCount,
                RefreshCount = req.RefreshCount,
                SessionDateEnd = DateTime.UtcNow,
                IsCompleted = true
            };

            await _activityService.UpdateActivityAsync(userId, activityId, updates);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping exploration activity");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("/api/users/{userId}/sessions/{sessionId}/stop")]
    public async Task<IActionResult> StopSession([FromRoute] string userId, [FromRoute] string sessionId)
    {
        try
        {
            await _activityService.StopSessionAsync(userId, sessionId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping session {SessionId}", sessionId);
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
        var session = a.UserSession;
        var source = a.Source ?? session?.TargetSource;

        return new UserActivityDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserSessionId = a.UserSessionId,
            Title = a.Title,
            Description = a.Description,
            SourceId = source?.Id ?? string.Empty,
            ExternalId = source?.ExternalId ?? string.Empty,
            SourceType = source?.Type ?? SourceType.Custom,
            DisplayTitle = source?.DisplayTitle ?? string.Empty,
            Url = source?.OnlineResource?.URL ?? (source?.Type == SourceType.Wikipedia ? $"https://en.wikipedia.org/wiki/{source.ExternalId}" : string.Empty),
            Type = a.Type,

            SessionDateStart = a.SessionDateStart,
            SessionDateEnd = a.SessionDateEnd,
            DurationSeconds = a.DurationSeconds,
            TotalDurationSeconds = a.TotalDurationSeconds,
            Score = a.Score,
            QuestionCount = a.QuestionCount,
            ScorePercentage = a.ScorePercentage,
            IsNewBestScore = a.IsNewBestScore,
            IsBaseline = a.IsBaseline,
            IsCompleted = a.IsCompleted,
            LlmModelName = a.LlmModelName,
            LlmVersion = a.LlmVersion,
            IsTracked = isTracked,
            Payload = a.Payload,
            ResultingReadActivityId = a.ResultingReadActivityId,
            ResultingReadSourceName = a.ResultingReadActivity?.Title,
            BacklogAddsCount = a.BacklogAddsCount,
            RefreshCount = a.RefreshCount
        };
    }

    public class ExploreRequest
    {
        public string? Title { get; set; }
        public string? SourceId { get; set; }
        public SourceType SourceType { get; set; }
        public string? SessionId { get; set; }
    }

    public class ReadRequest
    {
        public string? Title { get; set; }
        public string? Language { get; set; }
        public string? SourceId { get; set; }
        public SourceType SourceType { get; set; }
        public string? OriginExploreId { get; set; }
        public int? BacklogAddsCount { get; set; }
        public int? RefreshCount { get; set; }
        public int? ExploreDurationSeconds { get; set; }
    }

    public class StopExploreRequest
    {
        public int DurationSeconds { get; set; }
        public int? BacklogAddsCount { get; set; }
        public int? RefreshCount { get; set; }
    }
}
