using Xunit;
using Moq;
using DerotMyBrain.API.Services;
using DerotMyBrain.API.Repositories;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<ITrackedTopicRepository> _trackedTopicRepoMock;
    private readonly Mock<ILogger<ActivityService>> _loggerMock;
    private readonly ActivityService _service;
    
    public ActivityServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _trackedTopicRepoMock = new Mock<ITrackedTopicRepository>();
        _loggerMock = new Mock<ILogger<ActivityService>>();
        
        _service = new ActivityService(
            _activityRepoMock.Object,
            _trackedTopicRepoMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateActivityAsync_ReadSession_ShouldHaveNullScore()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Topic = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Read",
            Score = null,
            TotalQuestions = null
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal("Read", result.Type);
        Assert.Null(result.Score);
        Assert.Null(result.TotalQuestions);
        Assert.NotEqual(default(DateTime), result.SessionDate);
    }
    
    [Fact]
    public async Task CreateActivityAsync_QuizSession_ShouldHaveScore()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Topic = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Quiz",
            Score = 8,
            TotalQuestions = 10
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal("Quiz", result.Type);
        Assert.Equal(8, result.Score);
        Assert.Equal(10, result.TotalQuestions);
    }
    
    [Fact]
    public async Task CreateActivityAsync_TrackedTopic_ShouldUpdateCache()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Topic = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Quiz",
            Score = 9,
            TotalQuestions = 10
        };
        
        var trackedTopic = new TrackedTopic
        {
            Id = "tracked1",
            UserId = userId,
            Topic = "Test",
            BestScore = 6,
            TotalQuizAttempts = 1
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(userId, "Test"))
            .ReturnsAsync(true);
        
        _trackedTopicRepoMock.Setup(r => r.GetByTopicAsync(userId, "Test"))
            .ReturnsAsync(trackedTopic);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert - TrackedTopic should be updated
        _trackedTopicRepoMock.Verify(r => r.UpdateAsync(It.Is<TrackedTopic>(t =>
            t.BestScore == 9 && // New best score
            t.TotalQuizAttempts == 2
        )), Times.Once);
    }

    [Fact]
    public async Task CreateActivityAsync_NonTrackedTopic_ShouldNotUpdateCache()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto { Topic = "NewTopic", Type = "Quiz", Score = 10, TotalQuestions = 10 };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(userId, "NewTopic")).ReturnsAsync(false);
        
        // Act
        await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        _trackedTopicRepoMock.Verify(r => r.GetByTopicAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _trackedTopicRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TrackedTopic>()), Times.Never);
    }

    [Fact]
    public async Task CreateActivityAsync_NewBestScore_ShouldUpdateBestScoreDate()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto { Topic = "Test", Type = "Quiz", Score = 10, TotalQuestions = 10 };
        var trackedTopic = new TrackedTopic { UserId = userId, Topic = "Test", BestScore = 5, BestScoreDate = DateTime.UtcNow.AddDays(-1) };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _trackedTopicRepoMock.Setup(r => r.ExistsAsync(userId, "Test")).ReturnsAsync(true);
        _trackedTopicRepoMock.Setup(r => r.GetByTopicAsync(userId, "Test")).ReturnsAsync(trackedTopic);
        
        var beforeUpdate = DateTime.UtcNow;
        
        // Act
        await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        _trackedTopicRepoMock.Verify(r => r.UpdateAsync(It.Is<TrackedTopic>(t => 
            t.BestScore == 10 && 
            t.BestScoreDate >= beforeUpdate
        )), Times.Once);
    }
}
