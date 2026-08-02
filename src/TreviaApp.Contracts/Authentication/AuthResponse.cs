namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the authentication response payload returned after a successful login/refresh.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Initializes a new authentication response.
    /// </summary>
    public AuthResponse() { }

    /// <summary>
    /// Initializes a new authentication response.
    /// </summary>
    /// <param name="accessToken">JWT access token.</param>
    /// <param name="expiresAt">Access token expiration timestamp (UTC).</param>
    /// <param name="refreshToken">Refresh token.</param>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="email">Authenticated user email.</param>
    /// <param name="roles">Roles assigned to the authenticated user.</param>
    public AuthResponse(string accessToken, DateTimeOffset expiresAt, string refreshToken, Guid userId, string email, List<string> roles)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
        RefreshToken = refreshToken;
        UserId = userId;
        Email = email;
        Roles = roles;
    }

    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token expiration timestamp (UTC).
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authenticated user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the roles assigned to the authenticated user.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
