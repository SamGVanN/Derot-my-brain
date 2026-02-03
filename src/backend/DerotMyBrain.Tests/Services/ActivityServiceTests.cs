using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IWikipediaService> _wikipediaServiceMock;
    private readonly Mock<IQuizService> _quizServiceMock;
    private readonly Mock<ISourceService> _sourceServiceMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<ILogger<ActivityService>> _loggerMock;
    private readonly Mock<IContentSource> _contentSourceMock;
    private readonly ActivityService _service;
    private readonly Dictionary<string, Source> _sourceDb = new();
    private readonly Dictionary<string, OnlineResource> _onlineResourceDb = new();
    
    public ActivityServiceTests()
    {
        _activityRepoMock = new Mock<IActivityRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _wikipediaServiceMock = new Mock<IWikipediaService>();
        _quizServiceMock = new Mock<IQuizService>();
        _sourceServiceMock = new Mock<ISourceService>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _loggerMock = new Mock<ILogger<ActivityService>>();
        
        _contentSourceMock = new Mock<IContentSource>();
        _contentSourceMock.Setup(s => s.CanHandle(It.IsAny<SourceType>())).Returns(true);
        _contentSourceMock.Setup(s => s.GetContentAsync(It.IsAny<Source>()))
            .ReturnsAsync(new ContentResult { Title = "Test Title", TextContent = "Test Content" });

        var contentSources = new List<IContentSource> { _contentSourceMock.Object };

        _sourceServiceMock.Setup(s => s.PopulateSourceContentAsync(It.IsAny<Source>()))
            .Returns<Source>(async (s) => 
            {
                 var content = await _contentSourceMock.Object.GetContentAsync(s);
                 s.TextContent = content.TextContent;
            });

        // Setup stateful repository mocks for Source operations
        _activityRepoMock.Setup(r => r.GetSourceByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _sourceDb.TryGetValue(id, out var s) ? s : null);
        
        _activityRepoMock.Setup(r => r.CreateSourceAsync(It.IsAny<Source>()))
            .ReturnsAsync((Source s) => { _sourceDb[s.Id] = s; return s; });

        _activityRepoMock.Setup(r => r.UpdateSourceAsync(It.IsAny<Source>()))
            .ReturnsAsync((Source s) => { _sourceDb[s.Id] = s; return s; });

        // Setup stateful repository mocks for OnlineResource operations
        _activityRepoMock.Setup(r => r.GetOnlineResourceByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _onlineResourceDb.TryGetValue(id, out var r) ? r : null);

        _activityRepoMock.Setup(r => r.GetOnlineResourceBySourceIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string sourceId) => _onlineResourceDb.Values.FirstOrDefault(r => r.SourceId == sourceId));

        _activityRepoMock.Setup(r => r.CreateOnlineResourceAsync(It.IsAny<OnlineResource>()))
            .ReturnsAsync((OnlineResource r) => { _onlineResourceDb[r.Id] = r; return r; });

        _service = new ActivityService(
            _activityRepoMock.Object,
            _userRepoMock.Object,
            contentSources,
            _wikipediaServiceMock.Object,
            _quizServiceMock.Object,
            _sourceServiceMock.Object,
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

    [Fact]
    public async Task ReadAsync_ExistingSource_UsesResolvedSource()
    {
        // Arrange
        var userId = "user1";
        var sourceId = "existing-guid";
        var existingSource = new Source { Id = sourceId, Type = SourceType.Document, DisplayTitle = "Doc" };
        _sourceDb[sourceId] = existingSource;
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _service.ReadAsync(userId, "Title", "en", sourceId, SourceType.Document);

        // Assert
        Assert.Equal(sourceId, result.SourceId);
        _activityRepoMock.Verify(r => r.GetSourceByIdAsync(sourceId), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ReadAsync_NewWikipediaSource_AutoCreatesSourceAndOnlineResource()
    {
        // Arrange
        var userId = "user1";
        var wikiTitle = "Paris";
        var wikiUrl = "https://fr.wikipedia.org/wiki/Paris";
        var expectedHash = SourceHasher.GenerateId(SourceType.Wikipedia, wikiUrl);
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _service.ReadAsync(userId, "Paris Title", "fr", wikiTitle, SourceType.Wikipedia);

        // Assert
        Assert.NotEqual(expectedHash, result.SourceId); // Should be a new GUID
        
        // Verify Source created with correct ExternalId (Hash)
        _activityRepoMock.Verify(r => r.CreateSourceAsync(It.Is<Source>(s => s.ExternalId == expectedHash && s.Id != expectedHash)), Times.AtLeastOnce());
        
        // Verify OnlineResource created with correct ID (Hash)
        _activityRepoMock.Verify(r => r.CreateOnlineResourceAsync(It.Is<OnlineResource>(o => o.Id == expectedHash && o.URL == wikiUrl)), Times.AtLeastOnce());
        
        // Verify content update on source (the mock captures the created source via callback in Setup, but we need to ensure test accesses the captured one if _sourceDb logic holds)
        // In this test, we rely on _sourceDb being populated by CreateSourceAsync callback.
        // However, since result.SourceId is random, we can't look it up by technicalId anymore.
        // But we can check result.Source directly if populated in DTO or entity.
        
        // result is UserActivity, result.Source should be populated.
        Assert.NotNull(result.Source);
        Assert.Equal("Test Content", result.Source!.TextContent);
    }

    [Fact]
    public async Task ReadAsync_DeducesWikipediaTypeFromUrl()
    {
        // Arrange
        var userId = "user1";
        var wikiUrl = "https://en.wikipedia.org/wiki/France";
        var technicalId = SourceHasher.GenerateId(SourceType.Wikipedia, wikiUrl);
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _service.ReadAsync(userId, "France", "en", wikiUrl, null); // sourceType is null

        // Assert
        Assert.Equal(SourceType.Wikipedia, result.Source?.Type);
        _activityRepoMock.Verify(r => r.CreateSourceAsync(It.Is<Source>(s => s.Type == SourceType.Wikipedia)), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ReadAsync_WithQuizType_CreatesQuizActivity()
    {
        // Arrange
        var userId = "user1";
        var wikiTitle = "Paris";
        
        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _service.ReadAsync(userId, "Paris Title", "fr", wikiTitle, SourceType.Wikipedia, ActivityType.Quiz);

        // Assert
        Assert.Equal(ActivityType.Quiz, result.Type);
        Assert.Contains("Quiz session", result.Description);
    }

    [Fact]
    public async Task ReadAsync_WikipediaShouldReuseExistingSourceByExternalId()
    {
        // Arrange
        var userId = "user1";
        var wikiTitle = "London";
        var wikiUrl = "https://en.wikipedia.org/wiki/London";
        var expectedHash = SourceHasher.GenerateId(SourceType.Wikipedia, wikiUrl);
        
        // 1. Pre-create a source with the hash as ExternalId but a random GUID as Id
        var existingSource = new Source 
        { 
            Id = Guid.NewGuid().ToString(), 
            UserId = userId, 
            ExternalId = expectedHash, 
            Type = SourceType.Wikipedia, 
            DisplayTitle = "London" 
        };
        _sourceDb[existingSource.Id] = existingSource;
        
        // Setup mock to return this source when queried by ExternalId
        _activityRepoMock.Setup(r => r.GetSourceByExternalIdAsync(userId, expectedHash))
            .ReturnsAsync(existingSource);

        _activityRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);
        _activityRepoMock.Setup(r => r.UpdateAsync(It.IsAny<UserActivity>())).ReturnsAsync((UserActivity a) => a);

        // Act
        var result = await _service.ReadAsync(userId, "London Title", "en", wikiTitle, SourceType.Wikipedia);

        // Assert
        Assert.Equal(existingSource.Id, result.SourceId); // Should use the EXISTING GUID
        _activityRepoMock.Verify(r => r.CreateSourceAsync(It.IsAny<Source>()), Times.Never()); // Should NOT create a new source
    }
}
