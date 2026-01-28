using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<IWikipediaService> _wikipediaServiceMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<ILogger<ActivityService>> _loggerMock;
    private readonly ActivityService _service;
    
    public ActivityServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _wikipediaServiceMock = new Mock<IWikipediaService>();
        _llmServiceMock = new Mock<ILlmService>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _loggerMock = new Mock<ILogger<ActivityService>>();
        
        var contentSources = new List<IContentSource>();

        _service = new ActivityService(
            _activityRepoMock.Object,
            contentSources,
            _wikipediaServiceMock.Object,
            _llmServiceMock.Object,
            _jsonSerializerMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateActivityAsync_ReadSession_ShouldHaveSeparateDurations()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Test",
            SourceId = "https://test.com",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Read,
            DurationSeconds = 100,
            SessionDateStart = DateTime.UtcNow.AddMinutes(-2),
            SessionDateEnd = DateTime.UtcNow
        };
        
        _activityRepoMock.Setup(r => r.GetAllForContentAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(new List<UserActivity>());
            
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal(ActivityType.Read, result.Type);
        Assert.Equal(100, result.DurationSeconds);
        Assert.Equal(100, result.TotalDurationSeconds);
    }
    
    [Fact]
    public async Task CreateActivityAsync_QuizSession_ShouldCalculatePercentageAndCheckBest()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Test",
            SourceId = "https://test.com",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Quiz,
            Score = 8,
            QuestionCount = 10,
            DurationSeconds = 200,
            SessionDateStart = DateTime.UtcNow.AddMinutes(-5),
            SessionDateEnd = DateTime.UtcNow
        };
        
        _activityRepoMock.Setup(r => r.GetAllForContentAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(new List<UserActivity>()); // No previous best

        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal(ActivityType.Quiz, result.Type);
        Assert.Equal(8, result.Score);
        Assert.Equal(10, result.QuestionCount);
        Assert.Equal(80.0, result.ScorePercentage);
        Assert.True(result.IsNewBestScore);
    }
    
    [Fact]
    public async Task CreateActivityAsync_WithPreviousScore_ShouldIdentifyIfBest()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Test",
            SourceId = "https://test.com",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Quiz,
            Score = 7,
            QuestionCount = 10,
            SessionDateStart = DateTime.UtcNow,
            SessionDateEnd = DateTime.UtcNow
        };
        
        var history = new List<UserActivity>
        {
            new UserActivity { Id = "h1", UserId = userId, UserSessionId = "s1", Title = "...", Description = "...", Type = ActivityType.Quiz, ScorePercentage = 90.0 }
        };

        _activityRepoMock.Setup(r => r.GetAllForContentAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(history);

        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);
        
        // Act
        var result = await _service.CreateActivityAsync(userId, dto);
        
        // Assert
        Assert.Equal(70.0, result.ScorePercentage);
        Assert.False(result.IsNewBestScore); // 70 < 90
    }

    [Fact]
    public async Task CreateActivityAsync_ExploreSession_ShouldPersistBacklogAddsCount()
    {
        // Arrange
        var userId = "user1";
        var dto = new CreateActivityDto
        {
            Title = "Explore Test",
            SourceId = "derot://explore/2026-01-25T14:00:00Z",
            SourceType = SourceType.Wikipedia, // using existing enum value for test
            Type = ActivityType.Read, // Explore represented as a pre-read; service accepts Type
            SessionDateStart = DateTime.UtcNow,
            BacklogAddsCount = 3
        };

        _activityRepoMock.Setup(r => r.GetAllForContentAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(new List<UserActivity>());

        UserActivity? created = null;
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => { created = a; return a; });

        // Act
        var result = await _service.CreateActivityAsync(userId, dto);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(3, created!.BacklogAddsCount);
        Assert.Equal(3, result.BacklogAddsCount);
    }

    [Fact]
    public async Task UpdateActivityAsync_ShouldSetResultingReadActivityIdAndBacklogCount()
    {
        // Arrange
        var userId = "user1";
        var activityId = "act-1";
        var existing = new UserActivity
        {
            Id = activityId,
            UserId = userId,
            UserSessionId = "s1",
            Title = "Expl",
            Description = "Exploration",
            Type = ActivityType.Read
        };

        _activityRepoMock.Setup(r => r.GetByIdAsync(userId, activityId)).ReturnsAsync(existing);
        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        var dto = new UpdateActivityDto
        {
            ResultingReadActivityId = "read-123",
            BacklogAddsCount = 2
        };

        // Act
        var updated = await _service.UpdateActivityAsync(userId, activityId, dto);

        // Assert
        Assert.Equal("read-123", updated.ResultingReadActivityId);
        Assert.Equal(2, updated.BacklogAddsCount);
    }

    [Fact]
    public async Task CreateActivityAsync_WithOriginExploreId_ShouldLinkExploreToRead()
    {
        // Arrange
        var userId = "user1";
        var exploreId = "explore-1";
        var dto = new CreateActivityDto
        {
            Title = "Read from Explore",
            SourceId = "https://en.wikipedia.org/wiki/Test",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow,
            OriginExploreId = exploreId
        };

        var explore = new UserActivity
        {
            Id = exploreId,
            UserId = userId,
            UserSessionId = "s-explore",
            Title = "Exploration",
            Description = "Exploration session",
            Type = ActivityType.Read
        };

        _activityRepoMock.Setup(r => r.GetAllForContentAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(new List<UserActivity>());

        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        _activityRepoMock.Setup(r => r.GetByIdAsync(userId, exploreId))
            .ReturnsAsync(explore);

        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>()))
            .ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _service.CreateActivityAsync(userId, dto);

        // Assert
        _activityRepoMock.Verify(r => r.UpdateAsync(It.Is<UserActivity>(u => u.Id == exploreId && u.ResultingReadActivityId == result.Id)), Times.Once);
        Assert.Equal(result.Id, (await _activityRepoMock.Object.CreateAsync(result)).Id);
    }
}
