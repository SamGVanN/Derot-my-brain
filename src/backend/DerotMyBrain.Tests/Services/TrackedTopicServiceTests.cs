using Xunit;
using Moq;
using DerotMyBrain.API.Services;
using DerotMyBrain.API.Repositories;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Tests.Services;

public class TrackedTopicServiceTests
{
    private readonly Mock<ITrackedTopicRepository> _trackedTopicRepoMock;
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<ILogger<TrackedTopicService>> _loggerMock;
    private readonly TrackedTopicService _service;
    
    public TrackedTopicServiceTests()
    {
        _trackedTopicRepoMock = new Mock<ITrackedTopicRepository>();
        _activityRepoMock = new Mock<IActivityRepository>();
        _loggerMock = new Mock<ILogger<TrackedTopicService>>();
        
        _service = new TrackedTopicService(
            _trackedTopicRepoMock.Object,
            _activityRepoMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task TrackTopicAsync_NewTopic_ShouldRebuildHistory()
    {
        // Arrange
        var userId = "user1";
        var topic = "Quantum";
        var url = "https://en.wikipedia.org/wiki/Quantum_mechanics";
        
        var activities = new List<UserActivity>
        {
            new UserActivity { Topic = topic, Type = "Read", SessionDate = DateTime.UtcNow.AddDays(-2) },
            new UserActivity { Topic = topic, Type = "Quiz", Score = 7, TotalQuestions = 10, SessionDate = DateTime.UtcNow.AddDays(-1) }
        };
        
        _trackedTopicRepoMock.Setup(r => r.GetByTopicAsync(userId, topic)).ReturnsAsync((TrackedTopic)null);
        _activityRepoMock.Setup(r => r.GetAllForTopicAsync(userId, topic)).ReturnsAsync(activities);
        
        // Act
        var result = await _service.TrackTopicAsync(userId, topic, url);
        
        // Assert
        Assert.Equal(1, result.TotalReadSessions);
        Assert.Equal(1, result.TotalQuizAttempts);
        Assert.Equal(7, result.BestScore);
        _trackedTopicRepoMock.Verify(r => r.CreateAsync(It.IsAny<TrackedTopic>()), Times.Once);
    }
    
    [Fact]
    public async Task UntrackTopicAsync_ShouldDeleteTrackedTopic()
    {
        // Arrange
        var userId = "user1";
        var topic = "Quantum";
        var tracked = new TrackedTopic { Id = "tracked1", UserId = userId, Topic = topic };
        
        _trackedTopicRepoMock.Setup(r => r.GetByTopicAsync(userId, topic)).ReturnsAsync(tracked);
        
        // Act
        await _service.UntrackTopicAsync(userId, topic);
        
        // Assert
        _trackedTopicRepoMock.Verify(r => r.DeleteAsync("tracked1"), Times.Once);
    }
}
