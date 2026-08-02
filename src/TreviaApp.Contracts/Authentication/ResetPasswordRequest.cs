namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the ResetPasswordRequest contract.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Gets or sets User Id.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Confirm Password.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
