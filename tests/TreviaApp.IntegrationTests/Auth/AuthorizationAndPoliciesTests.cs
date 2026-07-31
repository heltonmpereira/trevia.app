namespace TreviaApp.IntegrationTests.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.IntegrationTests.Utilities;
using Xunit;

[Collection("Auth Integration Tests")]
public class AuthorizationAndPoliciesTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public AuthorizationAndPoliciesTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Protected_WithoutToken_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/auth/me");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Protected_WithValidToken_ReturnsCurrentUser()
    {
        var email = "prot_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        var register = await _client.RegisterNewStudentAsync(email);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        var login = await _client.LoginAsync(email, "Student123!");
        using var auth = _factory.CreateClient().WithBearer(login);
        var resp = await auth.GetAsync("/api/auth/me");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await resp.Content.ReadFromJsonAsync<CurrentUserResponse>();
        me.Should().NotBeNull();
        me!.Email.Should().Be(email);
        me.EmailConfirmed.Should().BeTrue();
        me.Roles.Should().Contain("Student");
    }
}
