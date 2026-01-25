using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Services;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class UserFocusServiceTests
{
    private readonly Mock<IUserFocusRepository> _userFocusRepoMock;
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly UserFocusService _service;
    
    public UserFocusServiceTests()
    {
        _userFocusRepoMock = new Mock<IUserFocusRepository>();
        _activityRepoMock = new Mock<IActivityRepository>();
        
        _service = new UserFocusService(
            _userFocusRepoMock.Object,
            _activityRepoMock.Object);
        
        _userFocusRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserFocus>()))
            .ReturnsAsync((UserFocus t) => t);
    }
    
    [Fact]
    public async Task TrackTopicAsync_NewFocus_ShouldRebuildHistoryUsingSourceHash()
    {
        // Arrange
        var userId = "user1";
        var sourceId = "https://en.wikipedia.org/wiki/Quantum_mechanics";
        var sourceType = SourceType.Wikipedia;
        var displayTitle = "Quantum Mastery";
        
        var activities = new List<UserActivity>
        {
            new UserActivity 
            { 
                UserId = userId,
                SourceId = sourceId,
                SourceType = sourceType,
                SourceHash = "quantum-hash",
                Title = "Quantum Mechanics", 
                Description = "Read session",
                Type = ActivityType.Read, 
                ReadDurationSeconds = 600,
                SessionDateEnd = DateTime.UtcNow.AddDays(-2) 
            },
            new UserActivity 
            { 
                UserId = userId,
                SourceId = sourceId,
                SourceType = sourceType,
                SourceHash = "quantum-hash",
                Title = "Quantum Mechanics", 
                Description = "Quiz session",
                Type = ActivityType.Quiz, 
                Score = 8, 
                QuestionCount = 10,
                ScorePercentage = 80.0,
                QuizDurationSeconds = 300,
                SessionDateEnd = DateTime.UtcNow.AddDays(-1) 
            }
        };
        
        _userFocusRepoMock.Setup(r => r.GetByHashAsync(userId, It.IsAny<string>()))
            .ReturnsAsync((UserFocus?)null);

        _userFocusRepoMock.SetupSequence(r => r.GetByHashAsync(userId, It.IsAny<string>()))
            .ReturnsAsync((UserFocus?)null) // Initial check
            .ReturnsAsync(new UserFocus { UserId = userId, SourceId = sourceId, SourceType = sourceType, SourceHash = "abc" }) // Rebuild check
            .ReturnsAsync(new UserFocus { UserId = userId, SourceId = sourceId, SourceType = sourceType, SourceHash = "abc", BestScore = 80.0 }); // Final return

        _activityRepoMock.Setup(r => r.GetAllForContentAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(activities);
        
        // Act
        var result = await _service.TrackTopicAsync(userId, sourceId, sourceType, displayTitle);
        
        // Assert
        Assert.NotNull(result);
        _userFocusRepoMock.Verify(r => r.CreateAsync(It.IsAny<UserFocus>()), Times.Once);
    }
}
