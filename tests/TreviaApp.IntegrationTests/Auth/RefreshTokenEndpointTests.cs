namespace TreviaApp.IntegrationTests.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.IntegrationTests.Utilities;
using Xunit;

[Collection("Auth Integration Tests")]
public class RefreshTokenEndpointTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public RefreshTokenEndpointTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Refresh_WithValidTokens_ReturnsNewTokens()
    {
        var email = "refresh_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        var register = await _client.RegisterNewStudentAsync(email);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        var login = await _client.LoginAsync(email, "Student123!");
        var resp = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest { AccessToken = login.AccessToken, RefreshToken = login.RefreshToken });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var renewed = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        renewed.Should().NotBeNull();
        renewed!.AccessToken.Should().NotBe(login.AccessToken);
        renewed.RefreshToken.Should().NotBe(login.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithRevokedRefresh_ReturnsUnauthorized()
    {
        var email = "refresh_rev_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        await _client.RegisterNewStudentAsync(email);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        var login = await _client.LoginAsync(email, "Student123!");
        using var authClient = _factory.CreateClient().WithBearer(login);
        await authClient.PostAsJsonAsync("/api/auth/revoke", new RevokeRefreshTokenRequest { RefreshToken = login.RefreshToken });

        var resp = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest { AccessToken = login.AccessToken, RefreshToken = login.RefreshToken });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
