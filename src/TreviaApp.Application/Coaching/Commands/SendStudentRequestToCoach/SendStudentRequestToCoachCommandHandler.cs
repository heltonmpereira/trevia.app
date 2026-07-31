namespace TreviaApp.Application.Coaching.Commands.SendStudentRequestToCoach;

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

public sealed class SendStudentRequestToCoachCommandHandler : ICommandHandler<SendStudentRequestToCoachCommand, CoachInviteResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SendStudentRequestToCoachCommandHandler> _logger;

    public SendStudentRequestToCoachCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<SendStudentRequestToCoachCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachInviteResponse> Handle(SendStudentRequestToCoachCommand request, CancellationToken cancellationToken)
    {
        var studentId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        if (studentId == request.CoachId)
            throw new DomainException(
                "Você não pode solicitar coaching a si mesmo.",
                ErrorCodes.CoachCannotInviteSelf);

        var coachExists = await _db.Set<AppUser>()
            .AnyAsync(u => u.Id == request.CoachId && !u.IsDeleted, cancellationToken);

        if (!coachExists)
            throw new DomainException(
                "Professor não encontrado.",
                ErrorCodes.CoachUserNotFound);

        var duplicatePending = await _db.Set<CoachStudentRequest>()
            .AnyAsync(r =>
                r.CoachId == request.CoachId &&
                r.StudentId == studentId &&
                r.Status == CoachRequestStatus.Pending,
                cancellationToken);

        if (duplicatePending)
            throw new DomainException(
                "Já existe uma solicitação pendente entre você e este professor.",
                ErrorCodes.CoachInviteDuplicatePending);

        var activeLinkExists = await _db.Set<CoachStudentLink>()
            .AnyAsync(l =>
                l.CoachId == request.CoachId &&
                l.StudentId == studentId &&
                l.IsActive,
                cancellationToken);

        if (activeLinkExists)
            throw new DomainException(
                "Já existe um vínculo ativo de coaching entre você e este professor.",
                ErrorCodes.CoachLinkAlreadyExists);

        var invite = new CoachStudentRequest(
            request.CoachId,
            studentId,
            CoachInviteDirection.StudentToCoach,
            request.Message,
            request.ExpiresInDays,
            null);

        _db.Set<CoachStudentRequest>().Add(invite);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("SendStudentRequestToCoachHandler: SaveChangesAsync explícito concluído InviteId={InviteId}", invite.Id);

        _logger.LogInformation(
            "StudentRequestSent InviteId={InviteId} StudentId={StudentId} CoachId={CoachId}",
            invite.Id,
            studentId,
            request.CoachId);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == request.CoachId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        var studentName = await _db.Set<AppUser>()
            .Where(u => u.Id == studentId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return CoachingMappings.MapInvite(invite, coachName!, null, studentName!, null);
    }
}
