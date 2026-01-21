using System.Net;
using System.Net.Http.Json;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.Tests.Integration;
using Xunit;

namespace DerotMyBrain.Tests.Controllers;

public class TrackedTopicsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TrackedTopicsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TrackTopic_ShouldReturnCreated()
    {
        // Arrange
        var userId = "test-user";
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
        var userId = "test-user";

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/tracked-topics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<TrackedTopicDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTopicEvolution_NonTracked_ShouldReturnNotFound()
    {
        // Arrange
        var userId = "test-user";
        var topic = "NonExistentTopic";

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/tracked-topics/{topic}/evolution");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
