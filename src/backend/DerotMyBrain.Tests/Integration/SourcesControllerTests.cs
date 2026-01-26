using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using DerotMyBrain.Tests.Fixtures;

namespace DerotMyBrain.Tests.Integration;

public class SourcesControllerTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly DatabaseFixture _dbFixture;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _userId = "test-user-integration"; // Must match seeded user

    public SourcesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _dbFixture = new DatabaseFixture(_factory);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.CleanupAsync();
        await _dbFixture.SeedDefaultTestDataAsync();

        // Login to get token
        var loginDto = new { Name = _userId }; 
        var response = await _client.PostAsJsonAsync("/api/users", loginDto);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);

        if (result != null && !string.IsNullOrEmpty(result.Token))
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
        }
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    [Fact]
    public async Task GetSources_ShouldReturnList_WhenTrackedIsTrue()
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{_userId}/sources?tracked=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var sources = await response.Content.ReadFromJsonAsync<IEnumerable<TrackedSourceDto>>(_jsonOptions);
        Assert.NotNull(sources);
        // We assume test user has some seeds or empty list is okay.
        // Given seed data usually has some activities, but maybe no tracked sources by default.
    }

    [Fact]
    public async Task TrackSource_ShouldAddSource_AndReturnDto()
    {
        // Arrange
        var request = new TrackSourceRequestDto
        {
            SourceId = "Quantum_Physics",
            DisplayTitle = "Quantum Physics",
            SourceType = SourceType.Wikipedia
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/users/{_userId}/sources", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var trackedSource = await response.Content.ReadFromJsonAsync<TrackedSourceDto>(_jsonOptions);
        Assert.NotNull(trackedSource);
        Assert.Equal("Quantum_Physics", trackedSource.SourceId); // Internal logic maps ExternalId to SourceId in DTO
        // Assert.True(trackedSource.IsTracked); // TrackedSourceDto doesn't explicitly have IsTracked because it's implicit in being in the list, but our API returns it. 
        // Wait, TrackedSourceDto doesn't have IsTracked boolean. It has IsPinned/IsArchived. 
        // But verifying it appears in list:
        
        var listResponse = await _client.GetAsync($"/api/users/{_userId}/sources?tracked=true");
        var list = await listResponse.Content.ReadFromJsonAsync<IEnumerable<TrackedSourceDto>>(_jsonOptions);
        Assert.Contains(list, s => s.SourceId == "Quantum_Physics");
    }
    
    [Fact]
    public async Task ToggleTracking_ShouldUntrack()
    {
        // Arrange - Ensure we have a tracked source first
        var request = new TrackSourceRequestDto
        {
            SourceId = "Untrack_Me",
            DisplayTitle = "Untrack Me",
            SourceType = SourceType.Wikipedia
        };
        var setupResponse = await _client.PostAsJsonAsync($"/api/users/{_userId}/sources", request);
        var dto = await setupResponse.Content.ReadFromJsonAsync<TrackedSourceDto>(_jsonOptions);
        var technicalId = dto.Id;

        // Act - Untrack
        var patchResponse = await _client.PatchAsJsonAsync($"/api/users/{_userId}/sources/{technicalId}/track", new { isTracked = false });

        // Assert
        patchResponse.EnsureSuccessStatusCode();
        
        // Verify it's gone from tracked list
        var listResponse = await _client.GetAsync($"/api/users/{_userId}/sources?tracked=true");
        var list = await listResponse.Content.ReadFromJsonAsync<IEnumerable<TrackedSourceDto>>(_jsonOptions);
        Assert.DoesNotContain(list, s => s.Id == technicalId);
    }
}
