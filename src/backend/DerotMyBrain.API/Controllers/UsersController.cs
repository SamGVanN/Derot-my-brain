using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        return Ok(await _userService.GetAllUsersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateOrGetUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateOrGetUserAsync(request.Name, request.Language, request.PreferredTheme);
        return Ok(user);
    }
    
    // Helper Class for Request
    public class CreateUserRequest {
        public string Name { get; set; } = string.Empty;
        public string? Language { get; set; }
        public string? PreferredTheme { get; set; }
    }
}
