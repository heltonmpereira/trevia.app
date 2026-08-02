namespace TreviaApp.Domain.Identity;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Represents the AppUser domain entity.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets First Name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets Last Name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Gets Display Name.
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// Gets Created At.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>
    /// Gets Updated At.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
    /// <summary>
    /// Gets Is Deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// Gets Last Active At.
    /// </summary>
    public DateTimeOffset? LastActiveAt { get; set; }
}
