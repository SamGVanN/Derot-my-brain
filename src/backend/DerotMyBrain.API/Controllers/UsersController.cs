using DerotMyBrain.API.Models;
using DerotMyBrain.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrSelectUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required");
            }

            var user = await _userService.CreateOrGetUserAsync(request.Name, request.Language, request.PreferredTheme);
            return Ok(user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("{id}/preferences")]
        public async Task<IActionResult> GetPreferences(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user.Preferences);
        }

        [HttpPut("{id}/preferences")]
        public async Task<IActionResult> UpdatePreferences(string id, [FromBody] UserPreferences preferences)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Preferences = preferences;
            var updatedUser = await _userService.UpdateUserAsync(user);

            return Ok(updatedUser);
        }

        [HttpPatch("{id}/preferences/general")]
        public async Task<IActionResult> UpdateGeneralPreferences(string id, [FromBody] DTOs.GeneralPreferencesDto dto)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Preferences.Language = dto.Language;
            user.Preferences.PreferredTheme = dto.PreferredTheme;
            user.Preferences.QuestionCount = dto.QuestionCount;

            var updatedUser = await _userService.UpdateUserAsync(user);
            return Ok(updatedUser);
        }

        [HttpPatch("{id}/preferences/categories")]
        public async Task<IActionResult> UpdateCategoryPreferences(string id, [FromBody] DTOs.CategoryPreferencesDto dto)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Preferences.SelectedCategories = dto.SelectedCategories;

            var updatedUser = await _userService.UpdateUserAsync(user);
            return Ok(updatedUser);
        }
    }

    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Language { get; set; }
        public string? PreferredTheme { get; set; }
    }
}
