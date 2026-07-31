namespace TreviaApp.Application.Coaching.Commands.EndCoachRelationship;

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
using TreviaApp.Shared.Enums;

public sealed class EndCoachRelationshipCommandHandler : ICommandHandler<EndCoachRelationshipCommand, CoachStudentLinkResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EndCoachRelationshipCommandHandler> _logger;

    public EndCoachRelationshipCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<EndCoachRelationshipCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentLinkResponse> Handle(EndCoachRelationshipCommand request, CancellationToken cancellationToken)
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

        if (!isAdmin && userId != link.CoachId && userId != link.StudentId)
            throw new DomainException(
                "Você não tem permissão para encerrar este vínculo.",
                ErrorCodes.CoachLinkNotOwnerOfRelationship);

        var endReason = request.Reason;
        if (userId == link.CoachId)
            endReason = CoachRelationshipEndReason.EndedByCoach;
        else if (userId == link.StudentId)
            endReason = CoachRelationshipEndReason.EndedByStudent;
        else if (isAdmin)
            endReason = CoachRelationshipEndReason.EndedByAdmin;

        link.EndRelationship(userId, endReason, request.Notes);
        link.Delete();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("EndCoachRelationshipHandler: SaveChangesAsync explícito concluído LinkId={LinkId}", link.Id);

        _logger.LogInformation(
            "CoachRelationshipEnded LinkId={LinkId} EndedBy={UserId} Reason={Reason}",
            link.Id,
            userId,
            endReason);

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
