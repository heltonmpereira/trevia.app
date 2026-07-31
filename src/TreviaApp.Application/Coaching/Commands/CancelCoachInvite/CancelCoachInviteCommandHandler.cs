namespace TreviaApp.Application.Coaching.Commands.CancelCoachInvite;

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

public sealed class CancelCoachInviteCommandHandler : ICommandHandler<CancelCoachInviteCommand, CoachInviteResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CancelCoachInviteCommandHandler> _logger;

    public CancelCoachInviteCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CancelCoachInviteCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachInviteResponse> Handle(CancelCoachInviteCommand request, CancellationToken cancellationToken)
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
                "Este convite não está pendente e não pode ser cancelado.",
                ErrorCodes.CoachInviteNotPending);

        var isAdmin = _currentUser.IsInRole(AppRoles.Administrator)
                      || _currentUser.IsInRole(AppRoles.GymManager);

        Guid? senderId = invite.Direction == CoachInviteDirection.CoachToStudent
            ? invite.CoachId
            : invite.StudentId;

        if (!isAdmin && userId != senderId)
            throw new DomainException(
                "Você não tem permissão para cancelar este convite.",
                ErrorCodes.CoachInviteNotAuthorizedToCancel);

        invite.Cancel(userId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("CancelCoachInviteHandler: SaveChangesAsync explícito concluído InviteId={InviteId}", invite.Id);

        _logger.LogInformation(
            "CoachInviteCancelled InviteId={InviteId} CancelledBy={UserId}",
            invite.Id,
            userId);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == invite.CoachId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        var studentName = await _db.Set<AppUser>()
            .Where(u => u.Id == invite.StudentId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return CoachingMappings.MapInvite(invite, coachName!, null, studentName!, null);
    }
}
