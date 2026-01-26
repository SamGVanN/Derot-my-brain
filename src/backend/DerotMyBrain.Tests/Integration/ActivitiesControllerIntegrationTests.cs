using System.Net;
using System.Net.Http.Json;
using Xunit;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Tests.Fixtures;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Tests.Integration;

public class ActivitiesControllerIntegrationTests : 
    IClassFixture<CustomWebApplicationFactory>, 
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;

    public ActivitiesControllerIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task GetActivities_ShouldReturn200_WithActivities()
    {
        var response = await _client.GetAsync("/api/users/test-user-integration/activities");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var activities = await response.Content.ReadFromJsonAsync<List<UserActivityDto>>(_jsonOptions);
        Assert.NotNull(activities);
        Assert.Equal(2, activities.Count);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturn201_WithValidDto()
    {
        var dto = new CreateActivityDto
        {
            Title = "Mathematics",
            SourceId = "https://en.wikipedia.org/wiki/Mathematics",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Quiz,
            Score = 9,
            QuestionCount = 10,
            SessionDateStart = DateTime.UtcNow,
            SessionDateEnd = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/users/test-user-integration/activities", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserActivityDto>(_jsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Mathematics", created.Title);
    }

    [Fact]
    public async Task GetStatistics_ShouldReturn200_WithCorrectStats()
    {
        var response = await _client.GetAsync("/api/users/test-user-integration/statistics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsDto>(_jsonOptions);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TotalActivities);
        Assert.Equal(1, stats.UserFocusCount);
    }

    [Fact]
    public async Task CreateActivity_WithOriginExploreId_ShouldLinkExploreAndRead()
    {
        // Arrange: create an Explore activity directly in DB
        var explore = new UserActivity
        {
            Id = "explore-integ-1",
            UserId = "test-user-integration",
            UserSessionId = "session-physics", // Use a seeded session from SeedDefaultTestDataAsync
            Title = "Explore Session",
            Description = "Exploring topics",
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow.AddMinutes(-5)
        };

        await _dbFixture.SeedActivityAsync(explore);

        // Act: create a Read activity that references the Explore via OriginExploreId
        var dto = new CreateActivityDto
        {
            Title = "Linked Read",
            SourceId = "https://en.wikipedia.org/wiki/Linked",
            SourceType = SourceType.Wikipedia,
            Type = ActivityType.Read,
            SessionDateStart = DateTime.UtcNow,
            OriginExploreId = explore.Id
        };

        var postResponse = await _client.PostAsJsonAsync("/api/users/test-user-integration/activities", dto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<UserActivityDto>(_jsonOptions);
        Assert.NotNull(created);

        // Assert: fetch the explore activity and verify it was updated with ResultingReadActivityId
        var getResponse = await _client.GetAsync($"/api/users/test-user-integration/activities/{explore.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var exploreDto = await getResponse.Content.ReadFromJsonAsync<UserActivityDto>(_jsonOptions);
        Assert.NotNull(exploreDto);
        Assert.Equal(created.Id, exploreDto.ResultingReadActivityId);
    }
}
