namespace TreviaApp.Application.Coaching.Queries.GetMyStudents;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Profiles;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

public sealed class GetMyStudentsQueryHandler : IQueryHandler<GetMyStudentsQuery, CoachStudentsPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetMyStudentsQueryHandler> _logger;

    public GetMyStudentsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetMyStudentsQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentsPagedResponse> Handle(GetMyStudentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var query = _db.Set<CoachStudentLink>()
            .Include(l => l.Student)
            .Where(l => l.CoachId == userId);

        if (request.OnlyActive.HasValue)
        {
            query = query.Where(l => l.IsActive == request.OnlyActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var search = $"%{request.SearchName}%";
            query = query.Where(l =>
                EF.Functions.Like(l.Student.FirstName, search) ||
                EF.Functions.Like(l.Student.LastName, search) ||
                EF.Functions.Like(l.Student.UserName!, search));
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
                ordered = query.OrderBy(l => l.Student.FirstName).ThenBy(l => l.Student.LastName);
                break;
            case "namedesc":
                ordered = query.OrderByDescending(l => l.Student.FirstName).ThenByDescending(l => l.Student.LastName);
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

        var studentIds = items.Select(l => l.StudentId).Distinct().ToList();

        var studentProfiles = await _db.Set<UserProfile>()
            .Where(up => studentIds.Contains(up.UserId))
            .Select(up => new
            {
                up.UserId,
                up.Goal,
                up.Experience,
                PhotoFileId = up.Photo != null ? up.Photo.FileId : null
            })
            .ToDictionaryAsync(k => k.UserId, cancellationToken);

        var activePlanCounts = await _db.Set<TrainingPlan>()
            .Where(tp => tp.CreatedByUserId == userId
                && studentIds.Contains(tp.AssignedToStudentId!.Value)
                && tp.Status == TrainingPlanStatus.Active)
            .GroupBy(tp => tp.AssignedToStudentId!.Value)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.StudentId, v => v.Count, cancellationToken);

        var responseItems = new List<CoachStudentSummaryResponse>();
        foreach (var link in items)
        {
            var profile = studentProfiles.TryGetValue(link.StudentId, out var sp) ? sp : null;
            var activePlans = activePlanCounts.TryGetValue(link.StudentId, out var apc) ? apc : 0;

            responseItems.Add(new CoachStudentSummaryResponse(
                link.StudentId,
                link.Student.DisplayName ?? $"{link.Student.FirstName} {link.Student.LastName}",
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
