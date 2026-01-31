using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId}/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetSessions(string userId)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByUserIdAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<UserSessionDto>> GetSession(string userId, string sessionId)
    {
        try
        {
            var session = await _sessionService.GetSessionByIdAsync(userId, sessionId);
            if (session == null) return NotFound();
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId} for user {UserId}", sessionId, userId);
            return StatusCode(500, "Internal Server Error");
        }
    }
}
