namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the LoginRequest contract.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets Email.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Remember Me.
    /// </summary>
    public bool RememberMe { get; set; }
}
