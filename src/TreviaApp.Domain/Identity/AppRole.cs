namespace TreviaApp.Domain.Identity;

using Microsoft.AspNetCore.Identity;

public class AppRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public AppRole() { }

    public AppRole(string roleName) : base(roleName) { }
}
