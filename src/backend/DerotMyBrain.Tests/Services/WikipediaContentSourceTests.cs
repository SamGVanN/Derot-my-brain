using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Clients;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class WikipediaContentSourceTests
{
    private readonly Mock<ILogger<WikipediaContentSource>> _loggerMock;
    private readonly Mock<IWikipediaClient> _wikiClientMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly WikipediaContentSource _source;

    public WikipediaContentSourceTests()
    {
        _loggerMock = new Mock<ILogger<WikipediaContentSource>>();
        _wikiClientMock = new Mock<IWikipediaClient>();
        _userRepoMock = new Mock<IUserRepository>();
        
        _source = new WikipediaContentSource(
            _loggerMock.Object,
            _wikiClientMock.Object,
            _userRepoMock.Object
        );
    }

    [Fact]
    public async Task GetContentAsync_FromUrl_ExtractsLanguageAndTitle()
    {
        // Arrange
        var source = new Source 
        { 
            ExternalId = "https://fr.wikipedia.org/wiki/Paris",
            UserId = "user1"
        };
        
        _wikiClientMock.Setup(c => c.GetArticleRichContentAsync("Paris", "fr"))
            .ReturnsAsync(new ContentResult { Title = "Paris", TextContent = "Content" });

        // Act
        var result = await _source.GetContentAsync(source);

        // Assert
        _wikiClientMock.Verify(c => c.GetArticleRichContentAsync("Paris", "fr"), Times.Once);
        Assert.Equal("Paris", result.Title);
    }

    [Fact]
    public async Task GetContentAsync_FromTitle_UsesUserLanguage()
    {
        // Arrange
        var userId = "user1";
        var source = new Source 
        { 
            ExternalId = "London",
            UserId = userId
        };
        
        var user = new User 
        { 
            Id = userId, 
            Preferences = new UserPreferences { UserId = userId, Language = "de" } 
        };
        
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _wikiClientMock.Setup(c => c.GetArticleRichContentAsync("London", "de"))
            .ReturnsAsync(new ContentResult { Title = "London", TextContent = "Content" });

        // Act
        var result = await _source.GetContentAsync(source);

        // Assert
        _wikiClientMock.Verify(c => c.GetArticleRichContentAsync("London", "de"), Times.Once);
    }

    [Fact]
    public async Task GetContentAsync_FromTitle_FallbackToEnIfNoPrefs()
    {
        // Arrange
        var userId = "user1";
        var source = new Source 
        { 
            ExternalId = "Science",
            UserId = userId
        };
        
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);
        _wikiClientMock.Setup(c => c.GetArticleRichContentAsync("Science", "en"))
            .ReturnsAsync(new ContentResult { Title = "Science", TextContent = "Content" });

        // Act
        var result = await _source.GetContentAsync(source);

        // Assert
        _wikiClientMock.Verify(c => c.GetArticleRichContentAsync("Science", "en"), Times.Once);
    }
}
