using System.Net;
using System.Net.Http.Json;
using Xunit;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Tests.Fixtures;
using DerotMyBrain.Core.Utils;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Tests.Integration;

public class UserFocusControllerTests : 
    IClassFixture<CustomWebApplicationFactory>, 
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;

    public UserFocusControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _dbFixture = new DatabaseFixture(_factory);
        _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.CleanupAsync();
        await _dbFixture.SeedDefaultTestDataAsync();
        
        var loginDto = new { Name = "test-user-integration" };
        var response = await _client.PostAsJsonAsync("/api/users", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
        
        if (result != null && !string.IsNullOrEmpty(result.Token))
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
        }
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    [Fact]
    public async Task GetUserFocuses_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/users/test-user-integration/user-focus");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var focuses = await response.Content.ReadFromJsonAsync<List<UserFocusDto>>(_jsonOptions);
        Assert.NotNull(focuses);
        Assert.Single(focuses); 
    }

    [Fact]
    public async Task TrackTopic_ShouldReturn201()
    {
        var request = new
        {
            SourceId = "https://en.wikipedia.org/wiki/Mathematics",
            SourceType = SourceType.Wikipedia,
            DisplayTitle = "Maths Mastery"
        };

        var response = await _client.PostAsJsonAsync("/api/users/test-user-integration/user-focus", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserFocusDto>(_jsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Maths Mastery", created.DisplayTitle);
    }

    [Fact]
    public async Task UntrackTopic_ShouldReturn204()
    {
        var physicsId = SourceHasher.GenerateHash(SourceType.Wikipedia, "https://en.wikipedia.org/wiki/Physics");
        var response = await _client.DeleteAsync($"/api/users/test-user-integration/user-focus/{physicsId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
