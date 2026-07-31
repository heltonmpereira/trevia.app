namespace TreviaApp.Application.Coaching.Commands.SendCoachInvite;

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

public sealed class SendCoachInviteCommandHandler : ICommandHandler<SendCoachInviteCommand, CoachInviteResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SendCoachInviteCommandHandler> _logger;

    public SendCoachInviteCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<SendCoachInviteCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachInviteResponse> Handle(SendCoachInviteCommand request, CancellationToken cancellationToken)
    {
        var coachId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var isCoach = _currentUser.IsInRole(AppRoles.Trainer)
                      || _currentUser.IsInRole(AppRoles.Administrator)
                      || _currentUser.IsInRole(AppRoles.GymManager);

        if (!isCoach)
            throw new DomainException(
                "Apenas professores, administradores ou gerentes de academia podem enviar convites de coaching.",
                ErrorCodes.CoachRoleRequired);

        if (coachId == request.StudentId)
            throw new DomainException(
                "Você não pode convidar a si mesmo para coaching.",
                ErrorCodes.CoachCannotInviteSelf);

        var studentExists = await _db.Set<AppUser>()
            .AnyAsync(u => u.Id == request.StudentId && !u.IsDeleted, cancellationToken);

        if (!studentExists)
            throw new DomainException(
                "Aluno não encontrado.",
                ErrorCodes.StudentUserNotFound);

        var duplicatePending = await _db.Set<CoachStudentRequest>()
            .AnyAsync(r =>
                r.CoachId == coachId &&
                r.StudentId == request.StudentId &&
                r.Status == CoachRequestStatus.Pending,
                cancellationToken);

        if (duplicatePending)
            throw new DomainException(
                "Já existe um convite pendente entre você e este aluno.",
                ErrorCodes.CoachInviteDuplicatePending);

        var activeLinkExists = await _db.Set<CoachStudentLink>()
            .AnyAsync(l =>
                l.CoachId == coachId &&
                l.StudentId == request.StudentId &&
                l.IsActive,
                cancellationToken);

        if (activeLinkExists)
            throw new DomainException(
                "Já existe um vínculo ativo de coaching entre você e este aluno.",
                ErrorCodes.CoachLinkAlreadyExists);

        var invite = new CoachStudentRequest(
            coachId,
            request.StudentId,
            CoachInviteDirection.CoachToStudent,
            request.Message,
            request.ExpiresInDays,
            request.GrantedPermissionsOnAccept);

        _db.Set<CoachStudentRequest>().Add(invite);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("SendCoachInviteHandler: SaveChangesAsync explícito concluído InviteId={InviteId}", invite.Id);

        _logger.LogInformation(
            "CoachInviteSent InviteId={InviteId} CoachId={CoachId} StudentId={StudentId}",
            invite.Id,
            coachId,
            request.StudentId);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == coachId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        var studentName = await _db.Set<AppUser>()
            .Where(u => u.Id == request.StudentId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return CoachingMappings.MapInvite(invite, coachName!, null, studentName!, null);
    }
}
