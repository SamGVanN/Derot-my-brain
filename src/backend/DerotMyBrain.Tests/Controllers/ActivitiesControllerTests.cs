using DerotMyBrain.API.Controllers;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Controllers;

public class ActivitiesControllerTests
{
    [Fact]
    public async Task CreateActivity_ReturnsCreatedAndMapsExploreFields()
    {
        // Arrange
        var userId = "user1";
        var createDto = new CreateActivityDto
        {
            Title = "From Explore",
            SourceId = "https://en.wikipedia.org/wiki/Test",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow,
            BacklogAddsCount = 2
        };

        var createdActivity = new UserActivity
        {
            Id = "act-123",
            UserId = userId,
            UserSessionId = "s123",
            Title = createDto.Title,
            Description = createDto.Description,
            Type = createDto.Type,
            SessionDateStart = createDto.SessionDateStart,
            BacklogAddsCount = createDto.BacklogAddsCount
        };

        var activityServiceMock = new Mock<IActivityService>();
        activityServiceMock.Setup(s => s.CreateActivityAsync(userId, It.IsAny<CreateActivityDto>()))
            .ReturnsAsync(createdActivity);

        var userFocusMock = new Mock<IUserFocusService>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<ActivitiesController>>();

        var controller = new ActivitiesController(activityServiceMock.Object, userFocusMock.Object, loggerMock.Object);

        // Act
        var result = await controller.CreateActivity(userId, createDto);

        // Assert
        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<UserActivityDto>(createdAt.Value);
        Assert.Equal(createdActivity.Id, dto.Id);
        Assert.Equal(createdActivity.BacklogAddsCount, dto.BacklogAddsCount);
    }
}
