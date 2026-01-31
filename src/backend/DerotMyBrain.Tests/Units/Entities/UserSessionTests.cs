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

    [Fact]
    public void TotalDurationSeconds_ShouldSumActivityDurations()
    {
        // Arrange
        var session = new UserSession { UserId = "user1" };
        session.Activities.Add(new UserActivity { UserId = "user1", UserSessionId = session.Id, Title = "A1", Description = "D1", DurationSeconds = 100 });
        session.Activities.Add(new UserActivity { UserId = "user1", UserSessionId = session.Id, Title = "A2", Description = "D2", DurationSeconds = 200 });
        session.Activities.Add(new UserActivity { UserId = "user1", UserSessionId = session.Id, Title = "A3", Description = "D3", DurationSeconds = 50 });

        // Act
        var total = session.TotalDurationSeconds;

        // Assert
        Assert.Equal(350, total);
    }

    [Fact]
    public void TotalDurationSeconds_ShouldReturnZeroWhenNoActivities()
    {
        // Arrange
        var session = new UserSession { UserId = "user1" };

        // Act
        var total = session.TotalDurationSeconds;

        // Assert
        Assert.Equal(0, total);
    }
}
