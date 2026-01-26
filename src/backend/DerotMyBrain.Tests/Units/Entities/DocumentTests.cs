using DerotMyBrain.Core.Entities;
using Xunit;

namespace DerotMyBrain.Tests.Units.Entities;

public class DocumentTests
{
    [Fact]
    public void Constructor_ShouldInitializeId()
    {
        // Arrange & Act
        var doc = new Document 
        { 
            UserId = "user1", 
            FileName = "test.pdf",
            StoragePath = "path/to/file",
            DisplayTitle = "Test Document",
            FileType = ".pdf",
            SourceId = "source1"
        };

        // Assert
        Assert.False(string.IsNullOrEmpty(doc.Id));
        Assert.Equal("user1", doc.UserId);
        Assert.Equal("test.pdf", doc.FileName);
        // Checking if SourceId is initialized 
        Assert.Equal("source1", doc.SourceId);
    }

    [Fact]
    public void FileType_CanBeSet()
    {
        // Arrange
        var doc = new Document 
        { 
            UserId = "u", 
            FileName = "f", 
            FileType = ".pdf",
            StoragePath = "s",
            DisplayTitle = "t",
            SourceId = "s1"
        };

        // Assert
        Assert.Equal(".pdf", doc.FileType);
    }
}
