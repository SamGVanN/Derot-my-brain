using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using Xunit;

namespace DerotMyBrain.Tests.Integration;

public class SecurityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;

    public SecurityIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    }

    [Fact]
    public async Task GetActivities_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users/anyUser/activities");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsToken_And_AllowsAccess()
    {
        // Arrange
        var loginDto = new LoginDto { Name = "SecurityTestUser" };

        // Act 1: Login
        var loginResponse = await _client.PostAsJsonAsync("/api/users", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User.Id);

        // Act 2: Access Protected Resource
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{result.User.Id}/activities");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        
        var response = await _client.SendAsync(request);

        // Assert
        // Should be OK (200) or Empty list, but definitely Authorized.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAllUsers_IsPublic_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
