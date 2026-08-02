namespace TreviaApp.Domain.Identity;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Represents the AppRole domain entity.
/// </summary>
public class AppRole : IdentityRole<Guid>
{
    /// <summary>
    /// Gets Description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the AppRole class.
    /// </summary>
    public AppRole() { }

    /// <summary>
    /// Initializes a new instance of the AppRole class.
    /// </summary>
    public AppRole(string roleName) : base(roleName) { }
}
