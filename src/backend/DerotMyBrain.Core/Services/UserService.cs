using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;

namespace DerotMyBrain.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICategoryService _categoryService;

    public UserService(IUserRepository userRepository, ICategoryService categoryService)
    {
        _userRepository = userRepository;
        _categoryService = categoryService;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> CreateOrGetUserAsync(string name, string? language = null, string? preferredTheme = null)
    {
        var existingUser = await _userRepository.GetByNameAsync(name);
        if (existingUser != null)
        {
            existingUser.LastConnectionAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(existingUser);
            return existingUser;
        }

        var allCategories = await _categoryService.GetAllCategoriesAsync();

        var newUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = Guid.NewGuid().ToString(),
                QuestionsPerQuiz = 10,
                Theme = preferredTheme ?? "derot-brain",
                Language = language ?? "auto",
                FavoriteCategories = allCategories.Take(5).ToList() // Default to top 5 or logic
            }
        };
        newUser.Preferences.UserId = newUser.Id;

        return await _userRepository.CreateAsync(newUser);
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        // Add validation logic if needed
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        return await _userRepository.DeleteAsync(userId);
    }
}
