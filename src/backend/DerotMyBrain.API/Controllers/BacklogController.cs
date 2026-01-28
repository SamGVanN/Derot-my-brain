using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[Authorize]
[ApiController]
[Route("api/users/{userId}/backlog")]
public class BacklogController : ControllerBase
{
    private readonly IBacklogService _backlogService;
    private readonly ILogger<BacklogController> _logger;

    public BacklogController(IBacklogService backlogService, ILogger<BacklogController> logger)
    {
        _backlogService = backlogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BacklogItemDto>>> GetBacklog(string userId)
    {
        var items = await _backlogService.GetUserBacklogAsync(userId);
        var dtos = items.Select(i => new BacklogItemDto
        {
            Id = i.Id,
            UserId = i.UserId,
            SourceId = i.SourceId,
            SourceType = i.Source?.Type ?? SourceType.Wikipedia,
            Title = i.Title,
            AddedAt = i.AddedAt
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<BacklogItemDto>> AddToBacklog(string userId, [FromBody] AddToBacklogDto request)
    {
        var item = await _backlogService.AddToBacklogAsync(userId, request.SourceId, request.SourceType, request.Title, request.Url, request.Provider);
        
        var dto = new BacklogItemDto
        {
            Id = item.Id,
            UserId = item.UserId,
            SourceId = item.SourceId,
            SourceType = item.Source?.Type ?? SourceType.Wikipedia,
            Title = item.Title,
            AddedAt = item.AddedAt
        };

        return Ok(dto);
    }

    [HttpDelete("{sourceId}")]
    public async Task<IActionResult> RemoveFromBacklog(string userId, string sourceId)
    {
        await _backlogService.RemoveFromBacklogAsync(userId, sourceId);
        return NoContent();
    }
}
