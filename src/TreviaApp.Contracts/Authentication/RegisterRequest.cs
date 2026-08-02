namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the RegisterRequest contract.
/// </summary>
public class RegisterRequest
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
    /// Gets or sets Confirm Password.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets First Name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Last Name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
