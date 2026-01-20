using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;

namespace DerotMyBrain.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryService _categoryService;

        public UserService(IUserRepository userRepository, ICategoryService categoryService)
        {
            _userRepository = userRepository;
            _categoryService = categoryService;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.ToList();
        }

        public async Task<User> CreateOrGetUserAsync(string name, string? language = null, string? preferredTheme = null)
        {
            // Check if user exists
            var existingUser = await _userRepository.GetByNameAsync(name);
            if (existingUser != null)
            {
                existingUser.LastConnectionAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(existingUser);
                return existingUser;
            }

            // Fetch all categories to set as default for new user
            var allCategories = await _categoryService.GetAllCategoriesAsync();

            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CreatedAt = DateTime.UtcNow,
                LastConnectionAt = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    UserId = Guid.NewGuid().ToString(), // Will be overwritten by EF Core FK fixup or should be same as User.Id
                    // Actually, UserPreferences.UserId is the FK to User.Id. So it should be the same.
                    QuestionCount = 10,
                    PreferredTheme = preferredTheme ?? "derot-brain",
                    Language = language ?? "auto",
                    SelectedCategories = allCategories.Select(c => c.Id).ToList()
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
            var existingUser = await _userRepository.GetByIdAsync(user.Id);

            if (existingUser == null)
            {
                return null;
            }

            // Validation logic is preserved
            var allowedQuestionCounts = new[] { 5, 10, 15, 20 };
            if (user.Preferences != null)
            {
                if (!allowedQuestionCounts.Contains(user.Preferences.QuestionCount))
                {
                    user.Preferences.QuestionCount = 10;
                }

                var allowedLanguages = new[] { "en", "fr", "auto" };
                if (!string.IsNullOrEmpty(user.Preferences.Language) && 
                    !allowedLanguages.Contains(user.Preferences.Language))
                {
                    user.Preferences.Language = "auto";
                }
            }

            // The Repository UpdateAsync handles mapping values from the input user to the tracked entity
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<User?> UpdateUserNameAsync(string userId, string newName)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(newName));
            }

            if (newName.Length > 100)
            {
                throw new ArgumentException("Name cannot exceed 100 characters.", nameof(newName));
            }

            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.Name = newName;
            return await _userRepository.UpdateAsync(existingUser);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _userRepository.DeleteAsync(userId);
        }
    }
}
