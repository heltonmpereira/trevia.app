namespace TreviaApp.IntegrationTests.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.IntegrationTests.Utilities;
using Xunit;

[Collection("Auth Integration Tests")]
public class LoginEndpointTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public LoginEndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_WhenValidCredentials_ReturnsTokens()
    {
        var email = "login_ok_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        await _client.RegisterNewStudentAsync(email, "Student123!");
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = "Student123!", RememberMe = false });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNull();
        auth.Roles.Should().Contain("Student");
    }

    [Fact]
    public async Task Login_WhenInvalidCredentials_ReturnsUnauthorized()
    {
        var email = "notfound@test.com";
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = "WrongPass1!", RememberMe = false });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WhenEmailNotConfirmed_ReturnsForbidden()
    {
        var email = "unconfirmed_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        await _client.RegisterNewStudentAsync(email, "Student123!");
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = "Student123!", RememberMe = false });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
