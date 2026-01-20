using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using DerotMyBrain.API.Data;
using DerotMyBrain.API.DTOs;
using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using DerotMyBrain.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace DerotMyBrain.Tests.Integration;

/// <summary>
/// Integration tests for ActivitiesController using TestServer.
/// These tests verify the full HTTP request/response cycle with a real database (InMemory).
/// </summary>
public class ActivitiesControllerIntegrationTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;

    public ActivitiesControllerIntegrationTests()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                // Add DbContext with InMemory database
                services.AddDbContext<DerotDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid());
                });

                // Register repositories
                services.AddScoped<IActivityRepository, SqliteActivityRepository>();
                services.AddScoped<IUserRepository, SqliteUserRepository>();

                // Register services
                services.AddScoped<IActivityService, ActivityService>();
                services.AddLogging();

                // Add controllers
                services.AddControllers();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();

        // Get DbContext and seed data
        _scope = _server.Services.CreateScope();
        var context = _scope.ServiceProvider.GetRequiredService<DerotDbContext>();
        SeedTestData(context);
    }

    private static void SeedTestData(DerotDbContext context)
    {
        var testUser = new User
        {
            Id = "test-user-integration",
            Name = "Integration Test User",
            CreatedAt = DateTime.UtcNow,
            LastConnectionAt = DateTime.UtcNow,
            Preferences = new UserPreferences
            {
                UserId = "test-user-integration",
                QuestionCount = 10,
                PreferredTheme = "derot-brain",
                Language = "en"
            }
        };

        context.Users.Add(testUser);

        var activity1 = new UserActivity
        {
            Id = "activity-1",
            UserId = "test-user-integration",
            Topic = "Physics",
            WikipediaUrl = "https://en.wikipedia.org/wiki/Physics",
            Type = "Quiz",
            FirstAttemptDate = DateTime.UtcNow.AddDays(-5),
            LastAttemptDate = DateTime.UtcNow.AddDays(-5),
            LastScore = 8,
            BestScore = 8,
            TotalQuestions = 10,
            IsTracked = true
        };

        var activity2 = new UserActivity
        {
            Id = "activity-2",
            UserId = "test-user-integration",
            Topic = "History",
            WikipediaUrl = "https://en.wikipedia.org/wiki/History",
            Type = "Read",
            FirstAttemptDate = DateTime.UtcNow.AddDays(-2),
            LastAttemptDate = DateTime.UtcNow.AddDays(-2),
            IsTracked = false
        };

        context.Activities.AddRange(activity1, activity2);
        context.SaveChanges();
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
        Assert.Equal(8, activity.LastScore);
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
            LastScore = 9,
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
        // Arrange
        var dto = new UpdateActivityDto
        {
            LastScore = 10,
            TotalQuestions = 10
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/users/test-user-integration/activities/activity-1", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<UserActivityDto>();
        Assert.NotNull(updated);
        Assert.Equal(10, updated.LastScore);
        Assert.Equal(10, updated.BestScore); // Should update BestScore too
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
    public async Task GetTrackedTopics_ShouldReturn200_WithOnlyTrackedActivities()
    {
        // Act
        var response = await _client.GetAsync("/api/users/test-user-integration/tracked-topics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tracked = await response.Content.ReadFromJsonAsync<List<UserActivityDto>>();
        Assert.NotNull(tracked);
        Assert.Single(tracked);
        Assert.Equal("Physics", tracked[0].Topic);
        Assert.True(tracked[0].IsTracked);
    }

    [Fact]
    public async Task TrackActivity_ShouldReturn204()
    {
        // Act
        var response = await _client.PostAsync("/api/users/test-user-integration/activities/activity-2/track", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify tracking
        var getResponse = await _client.GetAsync("/api/users/test-user-integration/activities/activity-2");
        var activity = await getResponse.Content.ReadFromJsonAsync<UserActivityDto>();
        Assert.NotNull(activity);
        Assert.True(activity.IsTracked);
    }

    [Fact]
    public async Task UntrackActivity_ShouldReturn204()
    {
        // Act
        var response = await _client.DeleteAsync("/api/users/test-user-integration/activities/activity-1/track");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify untracking
        var getResponse = await _client.GetAsync("/api/users/test-user-integration/activities/activity-1");
        var activity = await getResponse.Content.ReadFromJsonAsync<UserActivityDto>();
        Assert.NotNull(activity);
        Assert.False(activity.IsTracked);
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
        Assert.Single(topScores); // Only one quiz activity with score
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _server?.Dispose();
    }
}
