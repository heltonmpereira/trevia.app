namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the ForgotPasswordRequest contract.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Gets or sets Email.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
