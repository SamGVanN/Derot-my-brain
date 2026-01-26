using DerotMyBrain.Core.Entities;
using Xunit;

namespace DerotMyBrain.Tests.Units.Entities;

public class UserSessionTests
{
    [Fact]
    public void Constructor_ShouldInitializeId()
    {
        // Arrange & Act
        var session = new UserSession { UserId = "user1" };

        // Assert
        Assert.False(string.IsNullOrEmpty(session.Id));
        Assert.Equal("user1", session.UserId);
        Assert.Equal(SessionStatus.Active, session.Status); // Assuming default is Active (0)
        Assert.Null(session.EndedAt); // Default value check should be null if nullable
    }

    [Fact]
    public void Status_ShouldBeSetCorrectly()
    {
        // Arrange
        var session = new UserSession { UserId = "user1", Status = SessionStatus.Stopped };

        // Assert
        Assert.Equal(SessionStatus.Stopped, session.Status);
    }
}
