using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Core.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<ITrackedTopicService> _trackedTopicServiceMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly ActivityService _service;
    
    public ActivityServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _trackedTopicServiceMock = new Mock<ITrackedTopicService>();
        _llmServiceMock = new Mock<ILlmService>();
        
        // Pass empty list for content sources as we are testing CreateActivityAsync mainly
        var contentSources = new List<IContentSource>();

        _service = new ActivityService(
            _activityRepoMock.Object,
            _trackedTopicServiceMock.Object,
            contentSources,
            _llmServiceMock.Object);
    }
    
    [Fact]
    public async Task CreateActivityAsync_ReadSession_ShouldHaveNullScore()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Read",
            Score = null,
            TotalQuestions = null
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal("Read", result.Type);
        Assert.Equal(0, result.Score); // Service defaults to 0
        Assert.Equal(0, result.MaxScore);
        Assert.NotEqual(default(DateTime), result.LastAttemptDate);
    }
    
    [Fact]
    public async Task CreateActivityAsync_QuizSession_ShouldHaveScore()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Quiz",
            Score = 8,
            TotalQuestions = 10
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal("Quiz", result.Type);
        Assert.Equal(8, result.Score);
        Assert.Equal(10, result.MaxScore);
    }
    
    [Fact]
    public async Task CreateActivityAsync_TrackedTopic_ShouldUpdateStats()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Test",
            WikipediaUrl = "https://test.com",
            Type = "Quiz",
            Score = 9,
            TotalQuestions = 10
        };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        // Act
        await _service.CreateActivityAsync(userId, dto);
        
        // Assert - Verify call to TrackedTopicService
        _trackedTopicServiceMock.Verify(s => s.UpdateStatsAsync(userId, "Test", It.Is<UserActivity>(a => 
            a.Score == 9 && a.MaxScore == 10
        )), Times.Once);
    }

    [Fact]
    public async Task CreateActivityAsync_NonTrackedTopic_ShouldStillCallUpdateButServiceHandlesIt()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto { Title = "NewTopic", Type = "Quiz", Score = 10, TotalQuestions = 10 };
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        
        // Act
        await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        _trackedTopicServiceMock.Verify(s => s.UpdateStatsAsync(userId, "NewTopic", It.IsAny<UserActivity>()), Times.Once);
    }
}
