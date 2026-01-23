using System.Net;
using System.Net.Http.Json;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Tests.Fixtures;
using Xunit;

namespace DerotMyBrain.Tests.Integration;

public class TrackedTopicsControllerTests : 
    IClassFixture<CustomWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly DatabaseFixture _dbFixture;

    public TrackedTopicsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _dbFixture = new DatabaseFixture(_factory);
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.CleanupAsync();
        await _dbFixture.SeedDefaultTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TrackTopic_ShouldReturnCreated()
    {
        // Arrange
        var userId = "test-user-integration";
        var dto = new 
        { 
            Topic = "C# Programming", 
            WikipediaUrl = "https://en.wikipedia.org/wiki/C_Sharp_(programming_language)" 
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/users/{userId}/tracked-topics", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TrackedTopicDto>();
        Assert.NotNull(result);
        Assert.Equal(dto.Topic, result.Topic);
    }

    [Fact]
    public async Task GetTrackedTopics_ShouldReturnList()
    {
        // Arrange
        var userId = "test-user-integration";

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/tracked-topics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<TrackedTopicDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetTopicEvolution_Tracked_ShouldReturnList()
    {
        // Arrange
        var userId = "test-user-integration";
        var topic = "Physics"; // Seeded in SeedDefaultTestDataAsync

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/tracked-topics/{topic}/evolution");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<UserActivityDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetTopicEvolution_NonTracked_ShouldReturnNotFound()
    {
        // Arrange
        var userId = "test-user-integration";
        var topic = "NonExistentTopic";

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/tracked-topics/{topic}/evolution");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UntrackTopic_ShouldReturnNoContent()
    {
        // Arrange
        var userId = "test-user-integration";
        var topic = "Physics";

        // Act
        var response = await _client.DeleteAsync($"/api/users/{userId}/tracked-topics/{topic}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var getResponse = await _client.GetAsync($"/api/users/{userId}/tracked-topics");
        var result = await getResponse.Content.ReadFromJsonAsync<List<TrackedTopicDto>>();
        Assert.DoesNotContain(result, t => t.Topic == topic);
    }
}
