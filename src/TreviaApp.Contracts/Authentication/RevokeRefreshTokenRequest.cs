namespace TreviaApp.Contracts.Authentication;

public class RevokeRefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
