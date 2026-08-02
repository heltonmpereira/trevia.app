namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the ConfirmEmailRequest contract.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// Gets or sets User Id.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
