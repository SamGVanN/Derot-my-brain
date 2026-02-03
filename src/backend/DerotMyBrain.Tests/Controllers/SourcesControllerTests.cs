using System;
using System.Threading.Tasks;
using DerotMyBrain.API.Controllers;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Controllers;

public class SourcesControllerTests
{
    private readonly Mock<ISourceService> _sourceServiceMock;
    private readonly Mock<ILogger<SourcesController>> _loggerMock;
    private readonly SourcesController _controller;

    public SourcesControllerTests()
    {
        _sourceServiceMock = new Mock<ISourceService>();
        _loggerMock = new Mock<ILogger<SourcesController>>();
        _controller = new SourcesController(_sourceServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetExtractionStatus_ValidSource_ReturnsStatus()
    {
        // Arrange
        var userId = "user-1";
        var sourceId = "source-1";
        var source = new Source
        {
            Id = sourceId,
            UserId = userId,
            Type = SourceType.Document,
            DisplayTitle = "Test Doc",
            ContentExtractionStatus = ContentExtractionStatus.Completed,
            ContentExtractionError = null,
            ContentExtractionCompletedAt = DateTime.UtcNow
        };

        _sourceServiceMock.Setup(s => s.GetSourceAsync(sourceId))
            .ReturnsAsync(source);

        // Act
        var result = await _controller.GetExtractionStatus(userId, sourceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SourcesController.ContentExtractionStatusDto>(okResult.Value);
        Assert.Equal(sourceId, dto.SourceId);
        Assert.Equal(ContentExtractionStatus.Completed, dto.Status);
        Assert.Null(dto.Error);
        Assert.NotNull(dto.CompletedAt);
    }

    [Fact]
    public async Task GetExtractionStatus_WrongUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "user-1";
        var sourceId = "source-1";
        var source = new Source
        {
            Id = sourceId,
            UserId = "different-user", // Different user
            Type = SourceType.Document,
            DisplayTitle = "Test Doc"
        };

        _sourceServiceMock.Setup(s => s.GetSourceAsync(sourceId))
            .ReturnsAsync(source);

        // Act
        var result = await _controller.GetExtractionStatus(userId, sourceId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Source not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetExtractionStatus_NonexistentSource_ReturnsNotFound()
    {
        // Arrange
        var userId = "user-1";
        var sourceId = "nonexistent-source";

        _sourceServiceMock.Setup(s => s.GetSourceAsync(sourceId))
            .ReturnsAsync((Source?)null);

        // Act
        var result = await _controller.GetExtractionStatus(userId, sourceId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Source not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetExtractionStatus_FailedExtraction_ReturnsErrorMessage()
    {
        // Arrange
        var userId = "user-1";
        var sourceId = "source-1";
        var errorMessage = "Failed to extract text from corrupted PDF";
        var source = new Source
        {
            Id = sourceId,
            UserId = userId,
            Type = SourceType.Document,
            DisplayTitle = "Test Doc",
            ContentExtractionStatus = ContentExtractionStatus.Failed,
            ContentExtractionError = errorMessage,
            ContentExtractionCompletedAt = DateTime.UtcNow
        };

        _sourceServiceMock.Setup(s => s.GetSourceAsync(sourceId))
            .ReturnsAsync(source);

        // Act
        var result = await _controller.GetExtractionStatus(userId, sourceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SourcesController.ContentExtractionStatusDto>(okResult.Value);
        Assert.Equal(ContentExtractionStatus.Failed, dto.Status);
        Assert.Equal(errorMessage, dto.Error);
    }
}
