using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateOrGetUserAsync(string name, string? language = null, string? preferredTheme = null);
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> UpdateUserAsync(User user);
    }
}
