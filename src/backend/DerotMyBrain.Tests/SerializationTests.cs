using System.Text.Json;
using DerotMyBrain.API.Models;
using Xunit;

namespace DerotMyBrain.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void UserSerialization_ShouldNotThrow_JsonException()
        {
            // Arrange
            var user = new User
            {
                Id = "test-user",
                Name = "Test User"
            };
            var preferences = new UserPreferences
            {
                UserId = "test-user",
                User = user
            };
            user.Preferences = preferences;

            // Act & Assert
            // This is expected to throw System.Text.Json.JsonException before the fix
            var exception = Record.Exception(() => JsonSerializer.Serialize(user));
            
            Assert.Null(exception);
        }
    }
}
