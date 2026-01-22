using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateOrGetUserAsync(string name, string? language = null, string? preferredTheme = null);
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> UpdateUserAsync(User user);
        Task<User?> UpdateUserNameAsync(string userId, string newName);
        Task<User?> UpdateDerotZonePreferencesAsync(string userId, int questionCount, List<string> selectedCategories);
        Task<User?> UpdateGeneralPreferencesAsync(string userId, string language, string preferredTheme);
        Task<bool> DeleteUserAsync(string userId);
    }
}
