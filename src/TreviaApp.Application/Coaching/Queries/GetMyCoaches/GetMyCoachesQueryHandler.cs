namespace TreviaApp.Application.Coaching.Queries.GetMyCoaches;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Profiles;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

public sealed class GetMyCoachesQueryHandler : IQueryHandler<GetMyCoachesQuery, CoachStudentsPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetMyCoachesQueryHandler> _logger;

    public GetMyCoachesQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetMyCoachesQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentsPagedResponse> Handle(GetMyCoachesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var query = _db.Set<CoachStudentLink>()
            .Include(l => l.Coach)
            .Where(l => l.StudentId == userId);

        if (request.OnlyActive.HasValue)
        {
            query = query.Where(l => l.IsActive == request.OnlyActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var search = $"%{request.SearchName}%";
            query = query.Where(l =>
                EF.Functions.Like(l.Coach.FirstName, search) ||
                EF.Functions.Like(l.Coach.LastName, search) ||
                EF.Functions.Like(l.Coach.UserName!, search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        IOrderedQueryable<CoachStudentLink> ordered;
        switch ((request.SortBy ?? "linkedSinceDesc").ToLowerInvariant())
        {
            case "linkedsinceasc":
                ordered = query.OrderBy(l => l.StartedAt);
                break;
            case "nameasc":
                ordered = query.OrderBy(l => l.Coach.FirstName).ThenBy(l => l.Coach.LastName);
                break;
            case "namedesc":
                ordered = query.OrderByDescending(l => l.Coach.FirstName).ThenByDescending(l => l.Coach.LastName);
                break;
            case "linkedSincedesc":
            default:
                ordered = query.OrderByDescending(l => l.StartedAt);
                break;
        }

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var coachIds = items.Select(l => l.CoachId).Distinct().ToList();

        var coachProfiles = await _db.Set<UserProfile>()
            .Where(up => coachIds.Contains(up.UserId))
            .Select(up => new
            {
                up.UserId,
                up.Goal,
                up.Experience,
                PhotoFileId = up.Photo != null ? up.Photo.FileId : null
            })
            .ToDictionaryAsync(k => k.UserId, cancellationToken);

        var activePlanCounts = await _db.Set<TrainingPlan>()
            .Where(tp => coachIds.Contains(tp.CreatedByUserId)
                && tp.AssignedToStudentId == userId
                && tp.Status == TrainingPlanStatus.Active)
            .GroupBy(tp => tp.CreatedByUserId)
            .Select(g => new { CoachId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.CoachId, v => v.Count, cancellationToken);

        var responseItems = new List<CoachStudentSummaryResponse>();
        foreach (var link in items)
        {
            var profile = coachProfiles.TryGetValue(link.CoachId, out var cp) ? cp : null;
            var activePlans = activePlanCounts.TryGetValue(link.CoachId, out var apc) ? apc : 0;

            responseItems.Add(new CoachStudentSummaryResponse(
                link.CoachId,
                link.Coach.DisplayName ?? $"{link.Coach.FirstName} {link.Coach.LastName}",
                profile?.PhotoFileId,
                profile?.Goal,
                profile?.Experience,
                link.StartedAt,
                link.Permissions,
                activePlans));
        }

        return new CoachStudentsPagedResponse
        {
            Items = responseItems,
            TotalCount = totalCount,
            PageIndex = page,
            PageSize = pageSize,
            HasNextPage = (page * pageSize) < totalCount
        };
    }
}
