namespace TreviaApp.Contracts.Authentication;

public class AuthResponse
{
    public AuthResponse() { }

    public AuthResponse(string accessToken, DateTimeOffset expiresAt, string refreshToken, Guid userId, string email, List<string> roles)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
        RefreshToken = refreshToken;
        UserId = userId;
        Email = email;
        Roles = roles;
    }

    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
