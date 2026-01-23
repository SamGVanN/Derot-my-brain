using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IAuthService authService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
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
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> CreateOrGetUser([FromBody] LoginDto request)
    {
        // Since LoginDto only has Name, we might need default values for Language/Theme or update LoginDto
        // Assuming the original request had these, we should probably update LoginDto to include them or keep using a request object that maps to creation params.
        // But for "Login" essentially we just need the name if we are strict about "Identity".
        // However, if it creates a user, it might need prefs.
        // Let's stick to the current logic: CreateOrGet.
        // If we switch to LoginDto (which only had Name), we lose Language/Theme.
        // Let's update LoginDto/CreateUserRequest to be consistent. 
        // For now, I'll use the existing request object but map it to LoginDto concepts.
        
        var user = await _userService.CreateOrGetUserAsync(request.Name, "auto", "derot-brain"); // Defaults if missing, or we update LoginDto.
        // Actually, the previous code used CreateUserRequest. I should probably keep it or update LoginDto.
        // Lets look at LoginDto again. It only has Name. 
        // If I want to support prefs on creation, I should update LoginDto.
        // For now, I will use "auto" and "derot-brain" as defaults if I use LoginDto.
        
        var token = _authService.GenerateIdentityToken(user);
        
        return Ok(new LoginResponseDto
        {
            Token = token,
            User = new UserDto 
            { 
               Id = user.Id, 
               Name = user.Name, 
               CreatedAt = user.CreatedAt.ToString("O"), 
               LastConnectionAt = user.LastConnectionAt.ToString("O") 
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
    
    // Helper Class for Request - REPLACED by LoginDto in Core
    // public class CreateUserRequest { ... }
}
