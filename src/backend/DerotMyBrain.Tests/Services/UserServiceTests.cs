using Moq;
using Xunit;
using FluentAssertions;
using DerotMyBrain.API.Services;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;

namespace DerotMyBrain.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IJsonRepository<UserList>> _mockRepository;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IJsonRepository<UserList>>();
            _mockCategoryService = new Mock<ICategoryService>();
            _userService = new UserService(_mockRepository.Object, _mockCategoryService.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { Id = "1", Name = "User1" },
                new User { Id = "2", Name = "User2" }
            };
            var userList = new UserList { Users = expectedUsers };
            
            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedUsers);
        }

        [Fact]
        public async Task CreateOrGetUserAsync_CreatesNewUser_WhenNotExists()
        {
            // Arrange
            var emptyUserList = new UserList { Users = new List<User>() };
            var categories = new List<WikipediaCategory>
            {
                new WikipediaCategory { Id = "culture-arts", Name = "Culture and the arts" },
                new WikipediaCategory { Id = "science", Name = "Science" }
            };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(emptyUserList);

            _mockCategoryService
                .Setup(cs => cs.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _userService.CreateOrGetUserAsync("NewUser");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("NewUser");
            result.Id.Should().NotBeNullOrEmpty();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.LastConnectionAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.Preferences.SelectedCategories.Should().Contain("culture-arts");
            result.Preferences.SelectedCategories.Should().Contain("science");
            result.Preferences.QuestionCount.Should().Be(10); // Default value

            // Verify repository was called to save
            _mockRepository.Verify(repo => repo.SaveAsync("users.json", It.IsAny<UserList>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrGetUserAsync_ReturnsExistingUser_WhenExists()
        {
            // Arrange
            var existingUser = new User 
            { 
                Id = "existing-id", 
                Name = "ExistingUser",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastConnectionAt = DateTime.UtcNow.AddDays(-1)
            };
            var userList = new UserList { Users = new List<User> { existingUser } };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.CreateOrGetUserAsync("ExistingUser");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("existing-id");
            result.Name.Should().Be("ExistingUser");
            result.CreatedAt.Should().Be(existingUser.CreatedAt);
            
            // Verify repository was called to save (to update LastConnectionAt)
            _mockRepository.Verify(repo => repo.SaveAsync("users.json", It.IsAny<UserList>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrGetUserAsync_UpdatesLastConnectionAt_ForExistingUser()
        {
            // Arrange
            var oldConnectionTime = DateTime.UtcNow.AddDays(-5);
            var existingUser = new User 
            { 
                Id = "user-id", 
                Name = "TestUser",
                LastConnectionAt = oldConnectionTime
            };
            var userList = new UserList { Users = new List<User> { existingUser } };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.CreateOrGetUserAsync("TestUser");

            // Assert
            result.LastConnectionAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.LastConnectionAt.Should().BeAfter(oldConnectionTime);
        }

        [Fact]
        public async Task CreateOrGetUserAsync_SetsAllCategoriesAsDefault_ForNewUser()
        {
            // Arrange
            var emptyUserList = new UserList { Users = new List<User>() };
            var allCategories = new List<WikipediaCategory>
            {
                new WikipediaCategory { Id = "cat1", Name = "Category 1" },
                new WikipediaCategory { Id = "cat2", Name = "Category 2" },
                new WikipediaCategory { Id = "cat3", Name = "Category 3" }
            };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(emptyUserList);

            _mockCategoryService
                .Setup(cs => cs.GetAllCategoriesAsync())
                .ReturnsAsync(allCategories);

            // Act
            var result = await _userService.CreateOrGetUserAsync("NewUser");

            // Assert
            result.Preferences.SelectedCategories.Should().HaveCount(3);
            result.Preferences.SelectedCategories.Should().Contain("cat1");
            result.Preferences.SelectedCategories.Should().Contain("cat2");
            result.Preferences.SelectedCategories.Should().Contain("cat3");
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
        {
            // Arrange
            var expectedUser = new User { Id = "test-id", Name = "TestUser" };
            var userList = new UserList { Users = new List<User> { expectedUser } };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.GetUserByIdAsync("test-id");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("test-id");
            result.Name.Should().Be("TestUser");
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var userList = new UserList { Users = new List<User>() };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.GetUserByIdAsync("non-existent-id");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_ValidatesQuestionCount_AndFallsBackToDefault()
        {
            // Arrange
            var user = new User 
            { 
                Id = "user-id", 
                Name = "TestUser",
                Preferences = new UserPreferences { QuestionCount = 999 } // Invalid
            };
            var userList = new UserList { Users = new List<User> { user } };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Preferences.QuestionCount.Should().Be(10); // Fallback to default
        }

        [Fact]
        public async Task UpdateUserAsync_ValidatesLanguage_AndFallsBackToAuto()
        {
            // Arrange
            var user = new User 
            { 
                Id = "user-id", 
                Name = "TestUser",
                Preferences = new UserPreferences { Language = "invalid-lang" }
            };
            var userList = new UserList { Users = new List<User> { user } };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Preferences.Language.Should().Be("auto"); // Fallback to auto
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUser_WhenValid()
        {
            // Arrange
            var user = new User 
            { 
                Id = "user-id", 
                Name = "UpdatedName",
                Preferences = new UserPreferences 
                { 
                    QuestionCount = 20,
                    Language = "fr",
                    PreferredTheme = "neo-wikipedia"
                }
            };
            var userList = new UserList { Users = new List<User> { user } };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("UpdatedName");
            result.Preferences.QuestionCount.Should().Be(20);
            result.Preferences.Language.Should().Be("fr");
            result.Preferences.PreferredTheme.Should().Be("neo-wikipedia");

            // Verify repository was called to save
            _mockRepository.Verify(repo => repo.SaveAsync("users.json", It.IsAny<UserList>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var user = new User { Id = "non-existent-id", Name = "TestUser" };
            var userList = new UserList { Users = new List<User>() };

            _mockRepository
                .Setup(repo => repo.GetAsync("users.json"))
                .ReturnsAsync(userList);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().BeNull();

            // Verify repository was NOT called to save
            _mockRepository.Verify(repo => repo.SaveAsync("users.json", It.IsAny<UserList>()), Times.Never);
        }
    }
}
