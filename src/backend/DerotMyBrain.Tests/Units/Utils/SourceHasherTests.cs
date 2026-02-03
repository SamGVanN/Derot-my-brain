using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Utils;
using Xunit;

namespace DerotMyBrain.Tests.Units.Utils;

public class SourceHasherTests
{
    [Fact]
    public void GenerateId_Document_ReturnsOriginalId()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString();
        
        // Act
        var result = SourceHasher.GenerateId(SourceType.Document, guid);
        
        // Assert
        Assert.Equal(guid, result);
    }

    [Fact]
    public void GenerateId_Wikipedia_ReturnsDeterministicHash()
    {
        // Arrange
        var title = "Artificial intelligence";
        
        // Act
        var result1 = SourceHasher.GenerateId(SourceType.Wikipedia, title);
        var result2 = SourceHasher.GenerateId(SourceType.Wikipedia, title);
        
        // Assert
        Assert.Equal(result1, result2);
        Assert.True(result1.Length > 10);
        Assert.DoesNotContain(" ", result1);
    }

    [Fact]
    public void GenerateId_Wikipedia_IsCaseInsensitive()
    {
        // Arrange
        var titleLower = "artificial intelligence";
        var titleMixed = "Artificial Intelligence";
        
        // Act
        var result1 = SourceHasher.GenerateId(SourceType.Wikipedia, titleLower);
        var result2 = SourceHasher.GenerateId(SourceType.Wikipedia, titleMixed);
        
        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GenerateId_Wikipedia_TrimsInput()
    {
        // Arrange
        var title = "  Paris  ";
        var expected = SourceHasher.GenerateId(SourceType.Wikipedia, "Paris");
        
        // Act
        var result = SourceHasher.GenerateId(SourceType.Wikipedia, title);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GenerateId_EmptyInput_ThrowsArgumentException(string? input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SourceHasher.GenerateId(SourceType.Wikipedia, input!));
    }
}
