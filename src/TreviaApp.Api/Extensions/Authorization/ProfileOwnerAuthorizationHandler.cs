using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Api.Extensions.Authorization;

public class ProfileOwnerAuthorizationHandler : AuthorizationHandler<IsProfileOwnerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsProfileOwnerRequirement requirement, Guid resourceProfileOwnerUserId)
    {
        if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
        {
            context.Fail(); return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirstValue(AppClaimTypes.UserId)
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdClaim, out var currentUserId))
        {
            if (currentUserId == resourceProfileOwnerUserId)
            {
                context.Succeed(requirement); return Task.CompletedTask;
            }
        }

        if (context.User.IsInRole(AppRoles.Administrator))
        {
            context.Succeed(requirement); return Task.CompletedTask;
        }

        if (context.User.IsInRole(AppRoles.GymManager))
        {
            context.Succeed(requirement); return Task.CompletedTask;
        }

        context.Fail(); return Task.CompletedTask;
    }
}
