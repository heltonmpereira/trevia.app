namespace TreviaApp.Application.Coaching.Commands.RejectCoachInvite;

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

public sealed class RejectCoachInviteCommandHandler : ICommandHandler<RejectCoachInviteCommand, CoachInviteResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RejectCoachInviteCommandHandler> _logger;

    public RejectCoachInviteCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<RejectCoachInviteCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachInviteResponse> Handle(RejectCoachInviteCommand request, CancellationToken cancellationToken)
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
                "Este convite não está pendente e não pode ser rejeitado.",
                ErrorCodes.CoachInviteNotPending);

        if (invite.IsExpired)
            throw new DomainException(
                "Este convite expirou e não pode ser rejeitado.",
                ErrorCodes.CoachInviteExpired);

        var isAdmin = _currentUser.IsInRole(AppRoles.Administrator)
                      || _currentUser.IsInRole(AppRoles.GymManager);

        Guid? recipientId = invite.Direction == CoachInviteDirection.CoachToStudent
            ? invite.StudentId
            : invite.CoachId;

        if (!isAdmin && userId != recipientId)
            throw new DomainException(
                "Você não tem permissão para rejeitar este convite.",
                ErrorCodes.CoachInviteNotAuthorizedToRespond);

        invite.Reject(userId, request.Reason);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("RejectCoachInviteHandler: SaveChangesAsync explícito concluído InviteId={InviteId}", invite.Id);

        _logger.LogInformation(
            "CoachInviteRejected InviteId={InviteId} RejectedBy={UserId}",
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
