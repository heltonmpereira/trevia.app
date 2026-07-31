namespace TreviaApp.Application.Coaching.Commands.UpdateCoachPermissions;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Coaching.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public sealed class UpdateCoachPermissionsCommandHandler : ICommandHandler<UpdateCoachPermissionsCommand, CoachStudentLinkResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateCoachPermissionsCommandHandler> _logger;

    public UpdateCoachPermissionsCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpdateCoachPermissionsCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentLinkResponse> Handle(UpdateCoachPermissionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var link = await _db.Set<CoachStudentLink>()
            .FirstOrDefaultAsync(l => l.Id == request.LinkId, cancellationToken);

        if (link is null)
            throw new DomainException(
                "Vínculo de coaching não encontrado.",
                ErrorCodes.CoachLinkNotFound);

        if (!link.IsActive)
            throw new DomainException(
                "Vínculo já está inativo.",
                ErrorCodes.CoachLinkAlreadyInactive);

        var isAdmin = _currentUser.IsInRole(AppRoles.Administrator)
                      || _currentUser.IsInRole(AppRoles.GymManager);

        if (!isAdmin && userId != link.CoachId)
            throw new DomainException(
                "Apenas o professor do vínculo ou administradores podem atualizar as permissões.",
                ErrorCodes.CoachRoleRequired);

        link.UpdatePermissions(request.Permissions, link.CoachId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpdateCoachPermissionsHandler: SaveChangesAsync explícito concluído LinkId={LinkId}", link.Id);

        _logger.LogInformation(
            "CoachPermissionsUpdated LinkId={LinkId} UpdatedBy={UserId}",
            link.Id,
            userId);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == link.CoachId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        var studentName = await _db.Set<AppUser>()
            .Where(u => u.Id == link.StudentId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return CoachingMappings.MapLink(link, coachName!, null, studentName!, null);
    }
}
