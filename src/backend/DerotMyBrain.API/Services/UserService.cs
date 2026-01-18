using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;

namespace DerotMyBrain.API.Services
{
    public class UserService : IUserService
    {
        private readonly IJsonRepository<UserList> _userRepository;
        private const string UsersFileName = "users.json";

        public UserService(IJsonRepository<UserList> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            return data.Users;
        }

        public async Task<User> CreateOrGetUserAsync(string name)
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            
            var existingUser = data.Users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existingUser != null)
            {
                return existingUser;
            }

            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CreatedAt = DateTime.UtcNow
            };


            data.Users.Add(newUser);
            await _userRepository.SaveAsync(UsersFileName, data);

            return newUser;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            return data.Users.FirstOrDefault(u => u.Id == id);
        }
    }
}
