namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the RefreshTokenRequest contract.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Gets or sets Access Token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Refresh Token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
