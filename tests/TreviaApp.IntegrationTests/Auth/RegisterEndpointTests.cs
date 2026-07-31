namespace TreviaApp.IntegrationTests.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.IntegrationTests.Utilities;
using Xunit;

[Collection("Auth Integration Tests")]
public class RegisterEndpointTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public RegisterEndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WhenValidData_ReturnsCreated_WithTokens()
    {
        var email = "valid.user+" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Student123!",
            ConfirmPassword = "Student123!",
            FirstName = "João",
            LastName = "da Silva",
            DisplayName = "João S."
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await resp.Content.ReadFromJsonAsync<Contracts.Authentication.AuthResponse>();
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        auth.Roles.Should().Contain("Student");
        auth.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WhenDuplicateEmail_ReturnsConflict()
    {
        var email = "dup_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        var req = new
        {
            Email = email,
            Password = "Student123!",
            ConfirmPassword = "Student123!",
            FirstName = "A",
            LastName = "B"
        };
        (await _client.PostAsJsonAsync("/api/auth/register", req)).EnsureSuccessStatusCode();
        var resp2 = await _client.PostAsJsonAsync("/api/auth/register", req);
        resp2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WhenWeakPassword_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "weakpass@test.com",
            Password = "1234",
            ConfirmPassword = "1234",
            FirstName = "A",
            LastName = "B"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WhenPasswordsDontMatch_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "nomatch@test.com",
            Password = "Student123!",
            ConfirmPassword = "Student1234!",
            FirstName = "A",
            LastName = "B"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WhenMissingFields_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "",
            Password = "",
            ConfirmPassword = "",
            FirstName = "",
            LastName = ""
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
