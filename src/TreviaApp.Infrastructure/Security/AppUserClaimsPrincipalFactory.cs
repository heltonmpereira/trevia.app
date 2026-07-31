namespace TreviaApp.Infrastructure.Security;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TreviaApp.Infrastructure.Identity;
using TreviaApp.Shared.Constants;

public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
{
    public AppUserClaimsPrincipalFactory(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!identity.HasClaim(c => c.Type == AppClaimTypes.UserId))
            identity.AddClaim(new Claim(AppClaimTypes.UserId, user.Id.ToString()));

        if (!string.IsNullOrWhiteSpace(user.DisplayName) && !identity.HasClaim(c => c.Type == AppClaimTypes.DisplayName))
            identity.AddClaim(new Claim(AppClaimTypes.DisplayName, user.DisplayName));
        else if (string.IsNullOrWhiteSpace(user.DisplayName))
            identity.AddClaim(new Claim(AppClaimTypes.DisplayName, $"{user.FirstName} {user.LastName}".Trim()));

        if (!identity.HasClaim(c => c.Type == ClaimTypes.GivenName))
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));

        if (!identity.HasClaim(c => c.Type == ClaimTypes.Surname))
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

        return identity;
    }
}
