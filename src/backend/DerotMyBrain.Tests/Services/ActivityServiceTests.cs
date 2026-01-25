using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Core.DTOs;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<IUserFocusService> _userFocusServiceMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly ActivityService _service;
    
    public ActivityServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _userFocusServiceMock = new Mock<IUserFocusService>();
        _llmServiceMock = new Mock<ILlmService>();
        
        var contentSources = new List<IContentSource>();

        _service = new ActivityService(
            _activityRepoMock.Object,
            _userFocusServiceMock.Object,
            contentSources,
            _llmServiceMock.Object);
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
            ReadDurationSeconds = 100,
            QuizDurationSeconds = 0,
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
        Assert.Equal(100, result.ReadDurationSeconds);
        Assert.Equal(0, result.QuizDurationSeconds);
        Assert.Equal(100, result.TotalDurationSeconds);
        Assert.NotNull(result.SourceHash);
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
            ReadDurationSeconds = 50,
            QuizDurationSeconds = 150,
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
            new UserActivity { UserId = userId, SourceId = "...", SourceType = SourceType.Wikipedia, SourceHash = "...", Title = "...", Description = "...", Type = ActivityType.Quiz, ScorePercentage = 90.0 }
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
}
