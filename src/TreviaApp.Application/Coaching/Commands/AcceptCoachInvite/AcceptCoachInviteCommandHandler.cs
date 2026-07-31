namespace TreviaApp.Application.Coaching.Commands.AcceptCoachInvite;

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

public sealed class AcceptCoachInviteCommandHandler : ICommandHandler<AcceptCoachInviteCommand, CoachStudentLinkResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AcceptCoachInviteCommandHandler> _logger;

    public AcceptCoachInviteCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<AcceptCoachInviteCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentLinkResponse> Handle(AcceptCoachInviteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var invite = await _db.Set<CoachStudentRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.InviteId, cancellationToken);

        if (invite is null)
            throw new DomainException(
                "Convite de coaching não encontrado.",
                ErrorCodes.CoachInviteNotFound);

        if (invite.Status != CoachRequestStatus.Pending)
            throw new DomainException(
                "Este convite não está pendente e não pode ser aceito.",
                ErrorCodes.CoachInviteNotPending);

        if (invite.IsExpired)
            throw new DomainException(
                "Este convite expirou e não pode ser aceito.",
                ErrorCodes.CoachInviteExpired);

        var isAdmin = _currentUser.IsInRole(AppRoles.Administrator)
                      || _currentUser.IsInRole(AppRoles.GymManager);

        Guid? recipientId = invite.Direction == CoachInviteDirection.CoachToStudent
            ? invite.StudentId
            : invite.CoachId;

        if (!isAdmin && userId != recipientId)
            throw new DomainException(
                "Você não tem permissão para aceitar este convite.",
                ErrorCodes.CoachInviteNotAuthorizedToRespond);

        invite.Accept(userId);

        var link = new CoachStudentLink(
            invite.CoachId,
            invite.StudentId,
            invite.GrantedPermissionsOnAccept,
            invite.Id);

        _db.Set<CoachStudentLink>().Add(link);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AcceptCoachInviteHandler: SaveChangesAsync explícito concluído InviteId={InviteId} LinkId={LinkId}", invite.Id, link.Id);

        _logger.LogInformation(
            "CoachInviteAccepted InviteId={InviteId} LinkId={LinkId} AcceptedBy={UserId}",
            invite.Id,
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
