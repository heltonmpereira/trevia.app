namespace TreviaApp.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TreviaApp.Infrastructure.Identity;

public static class IdentityHelpers
{
    public static async Task ConfirmEmailAsync(TestWebApplicationFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = (await userMgr.FindByEmailAsync(email))!;
        var token = await userMgr.GenerateEmailConfirmationTokenAsync(user);
        await userMgr.ConfirmEmailAsync(user, token);
    }
    public static async Task<(Guid UserId, string Token)> GeneratePasswordResetTokenAsync(TestWebApplicationFactory factory, string email)
    {
        using var scope = factory.Services.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = (await userMgr.FindByEmailAsync(email))!;
        var token = await userMgr.GeneratePasswordResetTokenAsync(user);
        return (user.Id, token);
    }
}
