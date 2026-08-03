using TreviaApp.Contracts.Authentication;

namespace TreviaApp.Client.Services.Auth;

public interface IAuthService
{
    event Func<Task>? AuthenticationChangedAsync;

    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetAccessTokenAsync(bool forceRefresh = false);
    Task<string?> GetRefreshTokenAsync();
    Task<CurrentUserResponse?> GetCurrentUserCachedAsync();

    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokensAsync();
    Task LogoutAsync();

    Task ChangePasswordAsync(ChangePasswordRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ConfirmEmailAsync(ConfirmEmailRequest request);
    Task ResendConfirmationEmailAsync();

    Task<UserSessionsResponse> GetActiveSessionsAsync();
    Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request);
    Task RevokeAllSessionsAsync();
}
