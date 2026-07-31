using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Domain.Coaching;
using TreviaApp.Shared.Constants;

namespace TreviaApp.Api.Extensions.Authorization;

public class LinkedTrainerAuthorizationHandler : AuthorizationHandler<IsLinkedTrainerRequirement, Guid>
{
    private readonly IServiceProvider _serviceProvider;

    public LinkedTrainerAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsLinkedTrainerRequirement requirement,
        Guid resourceStudentId)
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
                l.CoachId == currentUserId &&
                l.StudentId == resourceStudentId &&
                l.IsActive);

        if (hasActiveLink)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}
