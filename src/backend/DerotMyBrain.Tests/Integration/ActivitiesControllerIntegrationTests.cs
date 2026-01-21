using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.Tests.Fixtures;

namespace DerotMyBrain.Tests.Integration;

/// <summary>
/// Integration tests for ActivitiesController using WebApplicationFactory.
/// These tests verify the full HTTP request/response cycle with InMemory database.
/// Uses IAsyncLifetime for proper async initialization and cleanup.
/// </summary>
public class ActivitiesControllerIntegrationTests : 
    IClassFixture<CustomWebApplicationFactory>, 
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly DatabaseFixture _dbFixture;

    public ActivitiesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _dbFixture = new DatabaseFixture(_factory);
    }

    /// <summary>
    /// Initialize test data before each test.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _dbFixture.CleanupAsync();
        await _dbFixture.SeedDefaultTestDataAsync();
    }

    /// <summary>
    /// Cleanup after each test (optional with InMemory DB, but good practice).
    /// </summary>
    public async Task DisposeAsync()
    {
        // Cleanup is optional since each test class gets a unique InMemory database
        // But we'll keep it for consistency and future-proofing
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetActivities_ShouldReturn200_WithActivities()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/activities");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var activities = await response.Content.ReadFromJsonAsync<List<UserActivityDto>>();
        Assert.NotNull(activities);
        Assert.Equal(2, activities.Count);
    }

    [Fact]
    public async Task GetActivity_ShouldReturn200_WhenActivityExists()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/activities/activity-1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var activity = await response.Content.ReadFromJsonAsync<UserActivityDto>();
        Assert.NotNull(activity);
        Assert.Equal("Physics", activity.Topic);
        Assert.Equal(8, activity.Score);
    }

    [Fact]
    public async Task GetActivity_ShouldReturn404_WhenActivityDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/activities/non-existent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturn201_WithValidDto()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Topic = "Mathematics",
            WikipediaUrl = "https://en.wikipedia.org/wiki/Mathematics",
            Type = "Quiz",
            Score = 9,
            TotalQuestions = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/test-user-integration/activities", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserActivityDto>();
        Assert.NotNull(created);
        Assert.Equal("Mathematics", created.Topic);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturn400_WithInvalidDto()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Topic = "", // Invalid - required
            WikipediaUrl = "not-a-url", // Invalid URL
            Type = "InvalidType" // Invalid type
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/test-user-integration/activities", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateActivity_ShouldReturn200_WithValidUpdate()
    {
        // Arrange - Create a dedicated activity for this test
        var testActivity = new Helpers.ActivityBuilder()
            .WithId("update-test-activity")
            .WithUserId("test-user-integration")
            .WithTopic("Mathematics")
            .AsQuiz(score: 7, totalQuestions: 10)
            .Build();
        
        await _dbFixture.SeedActivityAsync(testActivity);
        
        var dto = new UpdateActivityDto
        {
            Score = 10,
            TotalQuestions = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/users/test-user-integration/activities/update-test-activity", dto);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK, but got {response.StatusCode}. Content: {content}");
        
        var updated = await response.Content.ReadFromJsonAsync<UserActivityDto>();
        Assert.NotNull(updated);
        Assert.Equal(10, updated.Score);
    }

    [Fact]
    public async Task DeleteActivity_ShouldReturn204()
    {
        // Act
        var response = await _client.DeleteAsync("/api/users/test-user-integration/activities/activity-2");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync("/api/users/test-user-integration/activities/activity-2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetStatistics_ShouldReturn200_WithCorrectStats()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/statistics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsDto>();
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TotalActivities);
        Assert.Equal(1, stats.TrackedTopicsCount);
    }

    [Fact]
    public async Task GetActivityCalendar_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/statistics/activity-calendar?days=30");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var calendar = await response.Content.ReadFromJsonAsync<List<ActivityCalendarDto>>();
        Assert.NotNull(calendar);
    }

    [Fact]
    public async Task GetTopScores_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/statistics/top-scores?limit=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var topScores = await response.Content.ReadFromJsonAsync<List<TopScoreDto>>();
        Assert.NotNull(topScores);
    }

    [Fact]
    public async Task GetActivitiesByTopic_ShouldReturn200_WithEvolutionData()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/activities?topic=Physics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var activities = await response.Content.ReadFromJsonAsync<List<UserActivityDto>>();
        Assert.NotNull(activities);
        Assert.Single(activities); // Physics only has 1 activity in SeedDefaultTestDataAsync
        Assert.Equal("Physics", activities[0].Topic);
    }
}
