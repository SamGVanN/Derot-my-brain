using DerotMyBrain.Core.Entities;
using Xunit;

namespace DerotMyBrain.Tests.Units.Entities;

public class TopicTests
{
    [Fact]
    public void Constructor_ShouldInitializeId()
    {
        // Arrange & Act
        var topic = new Topic 
        { 
            UserId = "user1",
            Title = "My Topic"
        };

        // Assert
        Assert.False(string.IsNullOrEmpty(topic.Id));
        Assert.Equal("user1", topic.UserId);
        Assert.Equal("My Topic", topic.Title);
        Assert.Empty(topic.Sources);
    }
}
