using DerotMyBrain.Core.Entities;
using Xunit;

namespace DerotMyBrain.Tests.Units;

public class EntityTests
{
    [Fact]
    public void Source_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var source = new Source();

        // Assert
        Assert.NotNull(source.Activities);
        Assert.Empty(source.Activities);
        Assert.NotNull(source.Sessions);
        Assert.Empty(source.Sessions);
        Assert.NotNull(source.Documents);
        Assert.Empty(source.Documents);
        Assert.NotNull(source.BacklogItems);
        Assert.Empty(source.BacklogItems);
        Assert.False(source.IsTracked);
        Assert.Equal(string.Empty, source.Id); // Default string
    }

    [Fact]
    public void Source_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = "source-123";
        var title = "Quantum Physics";
        var type = SourceType.Wikipedia;

        // Act
        var source = new Source
        {
            Id = id,
            DisplayTitle = title,
            Type = type,
            IsTracked = true
        };

        // Assert
        Assert.Equal(id, source.Id);
        Assert.Equal(title, source.DisplayTitle);
        Assert.Equal(type, source.Type);
        Assert.True(source.IsTracked);
    }

    [Fact]
    public void UserSession_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var session = new UserSession { UserId = "user-1" };

        // Assert
        Assert.NotNull(session.Id); // Should be Guid
        Assert.NotEqual(string.Empty, session.Id);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.NotNull(session.Activities);
        Assert.Empty(session.Activities);
    }

    [Fact]
    public void Topic_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var topic = new Topic { UserId = "user-1", Title = "Science" };

        // Assert
        Assert.NotNull(topic.Id);
        Assert.NotEqual(string.Empty, topic.Id);
        Assert.Equal("Science", topic.Title);
        Assert.NotNull(topic.Sources);
        Assert.Empty(topic.Sources);
    }

    [Fact]
    public void Document_ShouldInitialize_And_StoreProperties()
    {
        // Arrange
        var doc = new Document
        {
            UserId = "user-1",
            FileName = "notes.pdf",
            StoragePath = "/data/notes.pdf",
            FileType = "application/pdf",
            FileSize = 1024,
            SourceId = "source-1"
        };

        // Assert
        Assert.NotNull(doc.Id);
        Assert.Equal("notes.pdf", doc.FileName);
        Assert.Equal("/data/notes.pdf", doc.StoragePath);
        Assert.Equal(1024, doc.FileSize);
        Assert.Equal("source-1", doc.SourceId);
    }
}
