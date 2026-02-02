using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace DerotMyBrain.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService, 
        IAuthService authService, 
        ICategoryService categoryService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _authService = authService;
        _categoryService = categoryService;
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
        var user = await _userService.CreateOrGetUserAsync(request.Name, request.Language, request.PreferredTheme);
        var token = _authService.GenerateIdentityToken(user);
        
        return Ok(new LoginResponseDto
        {
            Token = token,
            User = new UserDto 
            { 
               Id = user.Id, 
               Name = user.Name, 
               CreatedAt = user.CreatedAt.ToString("O"), 
               LastConnectionAt = user.LastConnectionAt.ToString("O"),
               Preferences = user.Preferences
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUserName(string id, [FromBody] UpdateUserDto request)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        user.Name = request.Name;
        var updatedUser = await _userService.UpdateUserAsync(user);
        
        if (updatedUser == null) return StatusCode(500, "Failed to update user");

        return Ok(updatedUser);
    }

    // --- Preferences Endpoints ---

    [HttpGet("{id}/preferences")]
    public async Task<ActionResult<UserPreferences>> GetPreferences(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null || user.Preferences == null) return NotFound();
        return Ok(user.Preferences);
    }

    [HttpPut("{id}/preferences")]
    public async Task<ActionResult<User>> UpdatePreferences(string id, [FromBody] UserPreferences preferences)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Preferences == null)
        {
            preferences.UserId = id;
            user.Preferences = preferences;
        }
        else
        {
            // Merge properties from the request to avoid losing existing data
            // This is safer than replacing the whole object if the client sends partial data
            if (!string.IsNullOrEmpty(preferences.Language)) user.Preferences.Language = preferences.Language;
            if (!string.IsNullOrEmpty(preferences.Theme)) user.Preferences.Theme = preferences.Theme;
            if (preferences.QuestionsPerQuiz > 0) user.Preferences.QuestionsPerQuiz = preferences.QuestionsPerQuiz;
            
            if (preferences.FavoriteCategories != null && preferences.FavoriteCategories.Any())
            {
                user.Preferences.FavoriteCategories = preferences.FavoriteCategories;
            }
        }

        var updatedUser = await _userService.UpdateUserAsync(user);
        return Ok(updatedUser);
    }

    [HttpPut("{id}/preferences/general")]
    public async Task<ActionResult<User>> UpdateGeneralPreferences(string id, [FromBody] GeneralPreferencesDto dto)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null || user.Preferences == null) return NotFound();

        if (dto.Language != null) user.Preferences.Language = dto.Language;
        if (dto.PreferredTheme != null) user.Preferences.Theme = dto.PreferredTheme;

        var updatedUser = await _userService.UpdateUserAsync(user);
        return Ok(updatedUser);
    }

    [HttpPut("{id}/preferences/derot-zone")]
    public async Task<ActionResult<User>> UpdateDerotZonePreferences(string id, [FromBody] DerotZonePreferencesDto dto)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null || user.Preferences == null) return NotFound();

        if (dto.QuestionCount.HasValue) user.Preferences.QuestionsPerQuiz = dto.QuestionCount.Value;
        
        if (dto.PreferredQuizFormat.HasValue) 
        {
            user.Preferences.PreferredQuizFormat = (QuizFormat)dto.PreferredQuizFormat.Value;
        }
        
        if (dto.SelectedCategories != null)
        {
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            user.Preferences.FavoriteCategories = allCategories
                .Where(c => dto.SelectedCategories.Contains(c.Id))
                .ToList();
        }
        
        var updatedUser = await _userService.UpdateUserAsync(user);
        return Ok(updatedUser);
    }

    [HttpPut("{id}/preferences/categories")]
    public async Task<ActionResult<User>> UpdateCategoryPreferences(string id, [FromBody] CategoryPreferencesDto dto)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null || user.Preferences == null) return NotFound();

        if (dto.SelectedCategories != null)
        {
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            user.Preferences.FavoriteCategories = allCategories
                .Where(c => dto.SelectedCategories.Contains(c.Id))
                .ToList();
        }
            
        var updatedUser = await _userService.UpdateUserAsync(user);
        return Ok(updatedUser);
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
