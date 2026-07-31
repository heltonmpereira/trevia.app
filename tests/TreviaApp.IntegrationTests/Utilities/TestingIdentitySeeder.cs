namespace TreviaApp.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public static class TestingIdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleMgr = services.GetRequiredService<RoleManager<AppRole>>();
        var roles = new[]
        {
            AppRoles.Administrator,
            AppRoles.Student,
            AppRoles.Trainer,
            AppRoles.GymManager
        };
        foreach (var r in roles)
        {
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new AppRole(r));
        }
    }
    public static async Task SeedAdminAsync(IServiceProvider services, string email, string password)
    {
        var userMgr = services.GetRequiredService<UserManager<AppUser>>();
        if (await userMgr.FindByEmailAsync(email) != null) return;
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = "Integration",
            LastName = "Admin",
            DisplayName = "Integra Admin",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var res = await userMgr.CreateAsync(user, password);
        if (res.Succeeded)
            await userMgr.AddToRoleAsync(user, AppRoles.Administrator);
    }
}
