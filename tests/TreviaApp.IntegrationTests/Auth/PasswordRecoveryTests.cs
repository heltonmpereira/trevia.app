namespace TreviaApp.IntegrationTests.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.IntegrationTests.Utilities;
using Xunit;

[Collection("Auth Integration Tests")]
public class PasswordRecoveryTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public PasswordRecoveryTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ForgotPassword_ExistingUser_ReturnsAccepted()
    {
        var email = "forgot_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        await _client.RegisterNewStudentAsync(email);
        var resp = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest { Email = email });
        resp.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task ForgotPassword_NonExistingUser_ReturnsAccepted_ToPreventEnumeration()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest { Email = "nonexistent@never.com" });
        resp.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task ForgotPassword_InvalidEmailFormat_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest { Email = "not-an-email" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ChangesPassword()
    {
        var email = "reset_" + Guid.NewGuid().ToString("N")[..6] + "@test.com";
        await _client.RegisterNewStudentAsync(email);
        var (userId, token) = await IdentityHelpers.GeneratePasswordResetTokenAsync(_factory, email);
        var newPass = "NewSuperPass99!";
        var resp = await _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest { UserId = userId.ToString(), Token = token, Password = newPass, ConfirmPassword = newPass });
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = newPass, RememberMe = false });
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
