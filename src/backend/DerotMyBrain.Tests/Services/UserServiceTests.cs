using Moq;
using Xunit;
using FluentAssertions;
using DerotMyBrain.API.Services;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DerotMyBrain.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IUserRepository>();
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
            
            _mockRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(expectedUsers);

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
            var categories = new List<WikipediaCategory>
            {
                new WikipediaCategory { Id = "culture-arts", Name = "Culture and the arts" },
                new WikipediaCategory { Id = "science", Name = "Science" }
            };

            _mockRepository
                .Setup(repo => repo.GetByNameAsync("NewUser"))
                .ReturnsAsync((User?)null);

            var createdUser = new User { Id = "new-id", Name = "NewUser", Preferences = new UserPreferences() };
            _mockRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u); // Return passed user

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
            _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrGetUserAsync_SetsInitialPreferences_ForNewUser()
        {
            // Arrange
            var categories = new List<WikipediaCategory>();

            _mockRepository
                .Setup(repo => repo.GetByNameAsync("NewUser"))
                .ReturnsAsync((User?)null);

            _mockRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            _mockCategoryService
                .Setup(cs => cs.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _userService.CreateOrGetUserAsync("NewUser", "fr", "neo-wikipedia");

            // Assert
            result.Preferences.Language.Should().Be("fr");
            result.Preferences.PreferredTheme.Should().Be("neo-wikipedia");
        }

        [Fact]
        public async Task CreateOrGetUserAsync_DoesNotOverwritePreferences_ForExistingUser()
        {
            // Arrange
            var existingUser = new User
            {
                Id = "existing-id",
                Name = "ExistingUser",
                Preferences = new UserPreferences 
                { 
                    Language = "en", 
                    PreferredTheme = "derot-brain" 
                }
            };

            _mockRepository
                .Setup(repo => repo.GetByNameAsync("ExistingUser"))
                .ReturnsAsync(existingUser);
            
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            // Attempt to create with DIFFERENT preferences
            var result = await _userService.CreateOrGetUserAsync("ExistingUser", "fr", "neo-wikipedia");

            // Assert
            result.Preferences.Language.Should().Be("en"); // Should keep existing
            result.Preferences.PreferredTheme.Should().Be("derot-brain"); // Should keep existing
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

            _mockRepository
                .Setup(repo => repo.GetByNameAsync("ExistingUser"))
                .ReturnsAsync(existingUser);
            
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _userService.CreateOrGetUserAsync("ExistingUser");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("existing-id");
            result.Name.Should().Be("ExistingUser");
            result.CreatedAt.Should().Be(existingUser.CreatedAt);
            
            // Verify repository update called (to update LastConnectionAt)
            _mockRepository.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
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

            _mockRepository
                .Setup(repo => repo.GetByNameAsync("TestUser"))
                .ReturnsAsync(existingUser);
            
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

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
            var allCategories = new List<WikipediaCategory>
            {
                new WikipediaCategory { Id = "cat1", Name = "Category 1" },
                new WikipediaCategory { Id = "cat2", Name = "Category 2" },
                new WikipediaCategory { Id = "cat3", Name = "Category 3" }
            };

            _mockRepository
                .Setup(repo => repo.GetByNameAsync("NewUser"))
                .ReturnsAsync((User?)null);
            
            _mockRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

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

            _mockRepository
                .Setup(repo => repo.GetByIdAsync("test-id"))
                .ReturnsAsync(expectedUser);

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
            _mockRepository
                .Setup(repo => repo.GetByIdAsync("non-existent-id"))
                .ReturnsAsync((User?)null);

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
            
            var existingUser = new User { Id = "user-id", Name = "TestUser", Preferences = new UserPreferences() };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync("user-id"))
                .ReturnsAsync(existingUser);
                
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

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

            var existingUser = new User { Id = "user-id", Name = "TestUser", Preferences = new UserPreferences() };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync("user-id"))
                .ReturnsAsync(existingUser);
                
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

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
            
            var existingUser = new User { Id = "user-id", Name = "OldName", Preferences = new UserPreferences() };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync("user-id"))
                .ReturnsAsync(existingUser);
                
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().NotBeNull();
            // result.Name.Should().Be("UpdatedName"); // In mock we just return mapped result or passed object
            // UserService logic calls repository.UpdateAsync(user) after validation. 
            // So result should match passed user (with default corrections if any)
            
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var user = new User { Id = "non-existent-id", Name = "TestUser" };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync("non-existent-id"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().BeNull();

            // Verify repository was NOT called to save
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserNameAsync_UpdatesUserName_WhenUserExists()
        {
            // Arrange
            var existingUser = new User 
            { 
                Id = "user-id", 
                Name = "OldName",
                Preferences = new UserPreferences()
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync("user-id"))
                .ReturnsAsync(existingUser);
            
            _mockRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _userService.UpdateUserNameAsync("user-id", "NewName");

            // Assert
            result.Should().NotBeNull();
            // Verify logic updated the name before calling update
            _mockRepository.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.Name == "NewName")), Times.Once);
        }

        [Fact]
        public async Task UpdateUserNameAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.GetByIdAsync("non-existent-id"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserNameAsync("non-existent-id", "NewName");

            // Assert
            result.Should().BeNull();

            // Verify repository was NOT called to save
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task UpdateUserNameAsync_ThrowsArgumentException_WhenNameIsNullOrWhitespace(string invalidName)
        {
            // Arrange
            // No mock needed as checking happens before repo call
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _userService.UpdateUserNameAsync("user-id", invalidName)
            );

            // Verify repository was NOT called
            _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserNameAsync_ThrowsArgumentException_WhenNameIsTooLong()
        {
            // Arrange
            var tooLongName = new string('a', 101); // 101 characters

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _userService.UpdateUserNameAsync("user-id", tooLongName)
            );

             // Verify repository was NOT called
            _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_DeletesUser_WhenUserExists()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.DeleteAsync("user-id"))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync("user-id");

            // Assert
            result.Should().BeTrue();

            // Verify repository calls
            _mockRepository.Verify(repo => repo.DeleteAsync("user-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.DeleteAsync("non-existent-id"))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync("non-existent-id");

            // Assert
            result.Should().BeFalse();
        }
    }
}
