namespace TreviaApp.Application.Coaching.Queries.GetPendingCoachRequestsCount;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Domain.Coaching;
using TreviaApp.Shared.Enums;

public sealed class GetPendingCoachRequestsCountQueryHandler : IQueryHandler<GetPendingCoachRequestsCountQuery, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetPendingCoachRequestsCountQueryHandler> _logger;

    public GetPendingCoachRequestsCountQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetPendingCoachRequestsCountQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<int> Handle(GetPendingCoachRequestsCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var count = await _db.Set<CoachStudentRequest>()
            .Where(r => r.Status == CoachRequestStatus.Pending)
            .Where(r =>
                (r.Direction == CoachInviteDirection.CoachToStudent && r.StudentId == userId) ||
                (r.Direction == CoachInviteDirection.StudentToCoach && r.CoachId == userId))
            .CountAsync(cancellationToken);

        return count;
    }
}
