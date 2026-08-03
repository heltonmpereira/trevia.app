namespace TreviaApp.IntegrationTests.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.IntegrationTests.Utilities;
using Xunit;

[Collection("Auth Integration Tests")]
public class FullAuthFlowTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public FullAuthFlowTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    private static string RandomEmail(string prefix) =>
        $"{prefix}_{Guid.NewGuid():N}[..6]@test.com".Replace("[..6]", string.Empty + Guid.NewGuid().ToString("N")[..6]);

    [Fact]
    public async Task HappyPath_RegisterConfirmLoginRefreshRevoke_Works()
    {
        var email = "fullflow_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";

        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Student123!",
            ConfirmPassword = "Student123!",
            FirstName = "Flow",
            LastName = "Test",
            DisplayName = "Flow T."
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerAuth = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();
        registerAuth.Should().NotBeNull();

        await IdentityHelpers.ConfirmEmailAsync(_factory, email);

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "Student123!",
            RememberMe = true
        });
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginAuth = await loginResp.Content.ReadFromJsonAsync<AuthResponse>();
        loginAuth.Should().NotBeNull();
        loginAuth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginAuth.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh-token", new RefreshTokenRequest
        {
            RefreshToken = loginAuth.RefreshToken
        });
        refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshAuth = await refreshResp.Content.ReadFromJsonAsync<AuthResponse>();
        refreshAuth.Should().NotBeNull();
        refreshAuth!.AccessToken.Should().NotBeNullOrWhiteSpace();

        var revokeResp = await _client.PostAsJsonAsync("/api/auth/revoke-refresh-token", new RevokeRefreshTokenRequest
        {
            RefreshToken = loginAuth.RefreshToken
        });
        revokeResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var reRefreshResp = await _client.PostAsJsonAsync("/api/auth/refresh-token", new RefreshTokenRequest
        {
            RefreshToken = loginAuth.RefreshToken
        });
        reRefreshResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PasswordRecovery_Flow_ResetSucceeds()
    {
        var email = "pwrec_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        await _client.RegisterNewStudentAsync(email, "Student123!");
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);

        var forgotResp = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest { Email = email });
        forgotResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.NoContent);

        var (userId, resetToken) = await IdentityHelpers.GeneratePasswordResetTokenAsync(_factory, email);
        userId.Should().NotBeEmpty();
        resetToken.Should().NotBeNullOrWhiteSpace();

        var resetResp = await _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
        {
            UserId = userId,
            Token = resetToken,
            NewPassword = "NewStudent123!",
            ConfirmPassword = "NewStudent123!"
        });
        resetResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginOld = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email, Password = "Student123!", RememberMe = false
        });
        loginOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var loginNew = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "NewStudent123!",
            RememberMe = false
        });
        loginNew.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
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
        var second = await _client.PostAsJsonAsync("/api/auth/register", req);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetCurrentUser_WithToken_ReturnsProfile()
    {
        var email = "curuser_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        var auth = await _client.RegisterNewStudentAsync(email, "Student123!");
        _client.WithBearer(auth);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);

        var me = await _client.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");
        me.Should().NotBeNull();
        me!.Email.Should().Be(email);
        me.UserId.Should().NotBeEmpty();
        me.Roles.Should().Contain("Student");
    }
}
