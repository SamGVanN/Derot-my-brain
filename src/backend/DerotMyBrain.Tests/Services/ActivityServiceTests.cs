using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using DerotMyBrain.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _mockRepository;
    private readonly Mock<ILogger<ActivityService>> _mockLogger;
    private readonly ActivityService _sut; // System Under Test

    public ActivityServiceTests()
    {
        _mockRepository = new Mock<IActivityRepository>();
        _mockLogger = new Mock<ILogger<ActivityService>>();
        _sut = new ActivityService(_mockRepository.Object, _mockLogger.Object);
    }

    #region CreateActivity

    [Fact]
    public async Task CreateActivity_ShouldSetBestScoreEqualToLastScore_WhenFirstAttempt()
    {
        // Arrange
        var userId = "test-user-1";
        var dto = new CreateActivityDto
        {
            Topic = "Test Topic",
            WikipediaUrl = "http://wiki",
            LastScore = 8,
            TotalQuestions = 10,
            Type = "Quiz"
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _sut.CreateActivityAsync(userId, dto);

        // Assert
        Assert.Equal(8, result.BestScore);
        Assert.Equal(8, result.LastScore);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<UserActivity>()), Times.Once);
    }

    [Fact]
    public async Task CreateReadActivity_ShouldSetTypeToRead()
    {
        // Arrange
        var userId = "test-user-1";
        var dto = new CreateActivityDto
        {
            Topic = "Test Topic",
            WikipediaUrl = "http://wiki",
            Type = "Read",
            TotalQuestions = 0,
            LastScore = 0
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _sut.CreateActivityAsync(userId, dto);

        // Assert
        Assert.Equal("Read", result.Type);
    }

    #endregion

    #region UpdateActivity

    [Fact]
    public async Task UpdateActivity_ShouldUpdateBestScore_WhenNewScoreIsHigher()
    {
        // Arrange
        var userId = "test-user-1";
        var activityId = "activity-1";
        var existingActivity = new UserActivity
        {
            Id = activityId,
            UserId = userId,
            BestScore = 5,
            LastScore = 5
        };

        var dto = new UpdateActivityDto
        {
            LastScore = 8,
            TotalQuestions = 10
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId, activityId))
            .ReturnsAsync(existingActivity);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _sut.UpdateActivityAsync(userId, activityId, dto);

        // Assert
        Assert.Equal(8, result.BestScore);
        Assert.Equal(8, result.LastScore);
    }

    [Fact]
    public async Task UpdateActivity_ShouldKeepBestScore_WhenNewScoreIsLower()
    {
        // Arrange
        var userId = "test-user-1";
        var activityId = "activity-1";
        var existingActivity = new UserActivity
        {
            Id = activityId,
            UserId = userId,
            BestScore = 15, // High score
            LastScore = 15
        };

        var dto = new UpdateActivityDto
        {
            LastScore = 10, // Lower score
            TotalQuestions = 20
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId, activityId))
            .ReturnsAsync(existingActivity);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _sut.UpdateActivityAsync(userId, activityId, dto);

        // Assert
        Assert.Equal(15, result.BestScore); // Should remain 15
        Assert.Equal(10, result.LastScore); // Should update to 10
    }

    [Fact]
    public async Task UpdateActivity_ShouldThrowNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange
        var userId = "test-user-1";
        var activityId = "non-existent";
        var dto = new UpdateActivityDto();

        _mockRepository.Setup(r => r.GetByIdAsync(userId, activityId))
            .ReturnsAsync((UserActivity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateActivityAsync(userId, activityId, dto));
    }

    #endregion

    #region Tracking

    [Fact]
    public async Task TrackActivity_ShouldCallRepository_WhenActivityExists()
    {
        // Arrange
        var userId = "test-user-1";
        var activityId = "activity-1";
        var activity = new UserActivity { Id = activityId, UserId = userId, IsTracked = false };

        _mockRepository.Setup(r => r.GetByIdAsync(userId, activityId))
            .ReturnsAsync(activity);

        // Act
        await _sut.TrackActivityAsync(userId, activityId);

        // Assert
        Assert.True(activity.IsTracked);
        _mockRepository.Verify(r => r.UpdateAsync(activity), Times.Once);
    }

    [Fact]
    public async Task UntrackActivity_ShouldCallRepository_WhenActivityExists()
    {
        // Arrange
        var userId = "test-user-1";
        var activityId = "activity-1";
        var activity = new UserActivity { Id = activityId, UserId = userId, IsTracked = true };

        _mockRepository.Setup(r => r.GetByIdAsync(userId, activityId))
            .ReturnsAsync(activity);

        // Act
        await _sut.UntrackActivityAsync(userId, activityId);

        // Assert
        Assert.False(activity.IsTracked);
        _mockRepository.Verify(r => r.UpdateAsync(activity), Times.Once);
    }

    #endregion

    #region Dashboard

    [Fact]
    public async Task GetStatistics_ShouldReturnDto_FromRepository()
    {
        // Arrange
        var userId = "test-user-1";
        var expectedStats = new UserStatisticsDto 
        { 
            TotalActivities = 42,
            TotalQuizzes = 20,
            TotalReads = 22
        };

        _mockRepository.Setup(r => r.GetStatisticsAsync(userId))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _sut.GetStatisticsAsync(userId);

        // Assert
        Assert.Same(expectedStats, result);
    }

    [Fact]
    public async Task GetActivityCalendar_ShouldReturnDto_FromRepository()
    {
        // Arrange
        var userId = "test-user-1";
        var expectedCalendar = new List<ActivityCalendarDto>
        {
            new() { Date = DateTime.Today, Count = 5 }
        };

        _mockRepository.Setup(r => r.GetActivityCalendarAsync(userId, 365))
            .ReturnsAsync(expectedCalendar);

        // Act
        var result = await _sut.GetActivityCalendarAsync(userId);

        // Assert
        Assert.Same(expectedCalendar, result);
    }

    [Fact]
    public async Task GetTopScores_ShouldReturnDto_FromRepository()
    {
        // Arrange
        var userId = "test-user-1";
        var expectedScores = new List<TopScoreDto>();

        _mockRepository.Setup(r => r.GetTopScoresAsync(userId, 10))
            .ReturnsAsync(expectedScores);

        // Act
        var result = await _sut.GetTopScoresAsync(userId);

        // Assert
        Assert.Same(expectedScores, result);
    }

    #endregion
}
