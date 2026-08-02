namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the RevokeRefreshTokenRequest contract.
/// </summary>
public class RevokeRefreshTokenRequest
{
    /// <summary>
    /// Gets or sets Refresh Token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
