using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DerotMyBrain.Tests.Controllers;

public class ActivitiesControllerTests
{
    private readonly Mock<DerotMyBrain.API.Services.IActivityService> _mockService;
    private readonly Mock<ILogger<DerotMyBrain.API.Controllers.ActivitiesController>> _mockLogger;
    private readonly DerotMyBrain.API.Controllers.ActivitiesController _controller;

    public ActivitiesControllerTests()
    {
        _mockService = new Mock<DerotMyBrain.API.Services.IActivityService>();
        _mockLogger = new Mock<ILogger<DerotMyBrain.API.Controllers.ActivitiesController>>();
        _controller = new DerotMyBrain.API.Controllers.ActivitiesController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetActivities_ShouldReturnOk_WhenActivitiesExist()
    {
        // Arrange
        var userId = "test-user";
        var activities = new List<DerotMyBrain.API.Models.UserActivity>
        {
            new DerotMyBrain.API.Models.UserActivity { Id = "1", UserId = userId, Topic = "Test Topic" }
        };
        _mockService.Setup(s => s.GetAllActivitiesAsync(userId))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetActivities(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedActivities = Assert.IsAssignableFrom<IEnumerable<DerotMyBrain.API.DTOs.UserActivityDto>>(okResult.Value);
        Assert.Single(returnedActivities);
    }

    [Fact]
    public async Task GetActivity_ShouldReturnOk_WhenFound()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "1";
        var activity = new DerotMyBrain.API.Models.UserActivity { Id = activityId, UserId = userId, Topic = "Test Topic" };

        _mockService.Setup(s => s.GetActivityByIdAsync(userId, activityId))
            .ReturnsAsync(activity);

        // Act
        var result = await _controller.GetActivity(userId, activityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<DerotMyBrain.API.DTOs.UserActivityDto>(okResult.Value);
        Assert.Equal(activityId, returnedDto.Id);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnCreated_WhenDtoIsValid()
    {
        // Arrange
        var userId = "test-user";
        var dto = new DerotMyBrain.API.DTOs.CreateActivityDto { Topic = "New Topic", WikipediaUrl = "http://wiki.com", Type = "Read" };
        var createdActivity = new DerotMyBrain.API.Models.UserActivity { Id = "1", UserId = userId, Topic = "New Topic" };

        _mockService.Setup(s => s.CreateActivityAsync(userId, dto))
            .ReturnsAsync(createdActivity);

        // Act
        var result = await _controller.CreateActivity(userId, dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedActivity = Assert.IsType<DerotMyBrain.API.DTOs.UserActivityDto>(createdResult.Value);
        Assert.Equal("New Topic", returnedActivity.Topic);
    }

    [Fact]
    public async Task GetActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "non-existent";
        _mockService.Setup(s => s.GetActivityByIdAsync(userId, activityId))
            .ReturnsAsync((DerotMyBrain.API.Models.UserActivity?)null);

        // Act
        var result = await _controller.GetActivity(userId, activityId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task TrackActivity_ShouldReturnNoContent()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "1";
        _mockService.Setup(s => s.TrackActivityAsync(userId, activityId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.TrackActivity(userId, activityId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnOk()
    {
        // Arrange
        var userId = "test-user";
        var stats = new DerotMyBrain.API.DTOs.UserStatisticsDto { TotalActivities = 5 };
        _mockService.Setup(s => s.GetStatisticsAsync(userId))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStats = Assert.IsType<DerotMyBrain.API.DTOs.UserStatisticsDto>(okResult.Value);
        Assert.Equal(5, returnedStats.TotalActivities);
    }
    [Fact]
    public async Task UpdateActivity_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "1";
        var dto = new DerotMyBrain.API.DTOs.UpdateActivityDto { LastScore = 10 };
        var updatedActivity = new DerotMyBrain.API.Models.UserActivity { Id = activityId, UserId = userId, LastScore = 10 };

        _mockService.Setup(s => s.UpdateActivityAsync(userId, activityId, dto))
            .ReturnsAsync(updatedActivity);

        // Act
        var result = await _controller.UpdateActivity(userId, activityId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<DerotMyBrain.API.DTOs.UserActivityDto>(okResult.Value);
        Assert.Equal(10, returnedDto.LastScore);
    }

    [Fact]
    public async Task UpdateActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "non-existent";
        var dto = new DerotMyBrain.API.DTOs.UpdateActivityDto();

        _mockService.Setup(s => s.UpdateActivityAsync(userId, activityId, dto))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.UpdateActivity(userId, activityId, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteActivity_ShouldReturnNoContent()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "1";
        _mockService.Setup(s => s.DeleteActivityAsync(userId, activityId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteActivity(userId, activityId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetTrackedTopics_ShouldReturnOk()
    {
        // Arrange
        var userId = "test-user";
        var activities = new List<DerotMyBrain.API.Models.UserActivity>
        {
            new DerotMyBrain.API.Models.UserActivity { Id = "1", UserId = userId, IsTracked = true }
        };
        _mockService.Setup(s => s.GetTrackedActivitiesAsync(userId))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetTrackedTopics(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedActivities = Assert.IsAssignableFrom<IEnumerable<DerotMyBrain.API.DTOs.UserActivityDto>>(okResult.Value);
        Assert.Single(returnedActivities);
    }

    [Fact]
    public async Task UntrackActivity_ShouldReturnNoContent()
    {
        // Arrange
        var userId = "test-user";
        var activityId = "1";
        _mockService.Setup(s => s.UntrackActivityAsync(userId, activityId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UntrackActivity(userId, activityId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetActivityCalendar_ShouldReturnOk()
    {
        // Arrange
        var userId = "test-user";
        var calendarData = new List<DerotMyBrain.API.DTOs.ActivityCalendarDto>();
        _mockService.Setup(s => s.GetActivityCalendarAsync(userId, 365))
            .ReturnsAsync(calendarData);

        // Act
        var result = await _controller.GetActivityCalendar(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsAssignableFrom<IEnumerable<DerotMyBrain.API.DTOs.ActivityCalendarDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetTopScores_ShouldReturnOk()
    {
        // Arrange
        var userId = "test-user";
        var topScores = new List<DerotMyBrain.API.DTOs.TopScoreDto>();
        _mockService.Setup(s => s.GetTopScoresAsync(userId, 10))
            .ReturnsAsync(topScores);

        // Act
        var result = await _controller.GetTopScores(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsAssignableFrom<IEnumerable<DerotMyBrain.API.DTOs.TopScoreDto>>(okResult.Value);
    }
}
