using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateOrGetUserAsync(string name);
    }
}
