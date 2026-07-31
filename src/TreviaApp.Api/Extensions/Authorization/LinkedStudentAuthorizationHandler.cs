using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Domain.Coaching;
using TreviaApp.Shared.Constants;

namespace TreviaApp.Api.Extensions.Authorization;

public class LinkedStudentAuthorizationHandler : AuthorizationHandler<IsLinkedStudentRequirement, Guid>
{
    private readonly IServiceProvider _serviceProvider;

    public LinkedStudentAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsLinkedStudentRequirement requirement,
        Guid resourceCoachId)
    {
        if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
        {
            context.Fail();
            return;
        }

        if (context.User.IsInRole(AppRoles.Administrator))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.User.IsInRole(AppRoles.GymManager))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = context.User.FindFirstValue(AppClaimTypes.UserId)
                       ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var currentUserId))
        {
            context.Fail();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var hasActiveLink = await db.Set<CoachStudentLink>()
            .AnyAsync(l =>
                l.StudentId == currentUserId &&
                l.CoachId == resourceCoachId &&
                l.IsActive);

        if (hasActiveLink)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}
