using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> CreateOrGetUserAsync(string name, string? language = null, string? preferredTheme = null);
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string userId);
}
