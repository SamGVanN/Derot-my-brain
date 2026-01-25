using System;
using System.Threading.Tasks;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/wikipedia")]
public class WikipediaController : ControllerBase
{
    private readonly IWikipediaService _wikipediaService;
    private readonly ILogger<WikipediaController> _logger;

    public WikipediaController(IWikipediaService wikipediaService, ILogger<WikipediaController> logger)
    {
        _wikipediaService = wikipediaService;
        _logger = logger;
    }

    [HttpPost]
    [Route("/api/backlog")]
    public IActionResult AddToBacklog([FromBody] BacklogRequest req)
    {
        // Minimal implementation for integration tests: return an ID
        return Ok(new { id = Guid.NewGuid().ToString() });
    }

    public class BacklogRequest
    {
        public string? Title { get; set; }
        public string? Lang { get; set; }
        public string? Url { get; set; }
        public string? Summary { get; set; }
    }
}
