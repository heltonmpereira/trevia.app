namespace TreviaApp.Application.Coaching.Queries.CheckCoachLinkStatus;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Shared.Enums;

public sealed class CheckCoachLinkStatusQueryHandler : IQueryHandler<CheckCoachLinkStatusQuery, CoachLinkStatusResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CheckCoachLinkStatusQueryHandler> _logger;

    public CheckCoachLinkStatusQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CheckCoachLinkStatusQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachLinkStatusResponse> Handle(CheckCoachLinkStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var otherUserId = request.OtherUserId;

        var activeLink = await _db.Set<CoachStudentLink>()
            .Where(l => l.IsActive)
            .Where(l =>
                (l.CoachId == userId && l.StudentId == otherUserId) ||
                (l.CoachId == otherUserId && l.StudentId == userId))
            .OrderByDescending(l => l.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var pendingInvite = await _db.Set<CoachStudentRequest>()
            .Where(r => r.Status == CoachRequestStatus.Pending)
            .Where(r =>
                ((r.CoachId == userId && r.StudentId == otherUserId) ||
                 (r.CoachId == otherUserId && r.StudentId == userId)))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        bool hasActiveLink = activeLink is not null;
        bool isCoachInRelationship = activeLink != null && activeLink.CoachId == userId;
        bool isStudentInRelationship = activeLink != null && activeLink.StudentId == userId;

        return new CoachLinkStatusResponse(
            otherUserId,
            hasActiveLink,
            activeLink?.Id,
            isCoachInRelationship,
            isStudentInRelationship,
            activeLink?.Permissions,
            pendingInvite?.Status,
            pendingInvite?.Id,
            pendingInvite?.Direction);
    }
}
