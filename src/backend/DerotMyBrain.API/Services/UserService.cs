using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;

namespace DerotMyBrain.API.Services
{
    public class UserService : IUserService
    {
        private readonly IJsonRepository<UserList> _userRepository;
        private readonly ICategoryService _categoryService;
        private const string UsersFileName = "users.json";

        public UserService(IJsonRepository<UserList> userRepository, ICategoryService categoryService)
        {
            _userRepository = userRepository;
            _categoryService = categoryService;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            return data.Users;
        }

        public async Task<User> CreateOrGetUserAsync(string name)
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            
            var existingUserIndex = data.Users.FindIndex(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existingUserIndex != -1)
            {
                var existingUser = data.Users[existingUserIndex];
                existingUser.LastConnectionAt = DateTime.UtcNow;
                data.Users[existingUserIndex] = existingUser;
                await _userRepository.SaveAsync(UsersFileName, data);
                return existingUser;
            }

            // Fetch all categories to set as default for new user
            var allCategories = await _categoryService.GetAllCategoriesAsync();

            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastConnectionAt = DateTime.UtcNow
            };
            
            // Default: All categories selected
            newUser.Preferences.SelectedCategories = allCategories.Select(c => c.Id).ToList();


            data.Users.Add(newUser);
            await _userRepository.SaveAsync(UsersFileName, data);

            return newUser;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            return data.Users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            var data = await _userRepository.GetAsync(UsersFileName);
            var existingUserIndex = data.Users.FindIndex(u => u.Id == user.Id);

            if (existingUserIndex == -1)
            {
                return null;
            }

            data.Users[existingUserIndex] = user;
            await _userRepository.SaveAsync(UsersFileName, data);
            return user;
        }
    }
}
