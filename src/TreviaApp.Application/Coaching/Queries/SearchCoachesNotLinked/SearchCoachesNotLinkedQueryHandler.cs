namespace TreviaApp.Application.Coaching.Queries.SearchCoachesNotLinked;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Enums;

public sealed class SearchCoachesNotLinkedQueryHandler : IQueryHandler<SearchCoachesNotLinkedQuery, CoachStudentsPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SearchCoachesNotLinkedQueryHandler> _logger;

    public SearchCoachesNotLinkedQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<SearchCoachesNotLinkedQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentsPagedResponse> Handle(SearchCoachesNotLinkedQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var trainerRoleIds = await _db.Set<IdentityRole<Guid>>()
            .Where(r => r.Name == AppRoles.Trainer)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var trainerUserIdsQuery = _db.Set<IdentityUserRole<Guid>>()
            .Where(ur => trainerRoleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId);

        var activeLinkedCoachIds = _db.Set<CoachStudentLink>()
            .Where(l => l.StudentId == userId && l.IsActive)
            .Select(l => l.CoachId);

        var pendingRequestCoachIds = _db.Set<CoachStudentRequest>()
            .Where(r => r.Status == CoachRequestStatus.Pending)
            .Where(r =>
                (r.Direction == CoachInviteDirection.StudentToCoach && r.StudentId == userId) ||
                (r.Direction == CoachInviteDirection.CoachToStudent && r.CoachId == userId))
            .Select(r => r.Direction == CoachInviteDirection.StudentToCoach ? r.CoachId : r.StudentId);

        var query = _db.Set<AppUser>()
            .Where(u => trainerUserIdsQuery.Contains(u.Id))
            .Where(u => u.Id != userId)
            .Where(u => !activeLinkedCoachIds.Contains(u.Id))
            .Where(u => !pendingRequestCoachIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var search = $"%{request.SearchName}%";
            query = query.Where(u =>
                EF.Functions.Like(u.FirstName, search) ||
                EF.Functions.Like(u.LastName, search) ||
                EF.Functions.Like(u.UserName!, search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = items.Select(u => u.Id).Distinct().ToList();

        var userProfiles = await _db.Set<UserProfile>()
            .Where(up => userIds.Contains(up.UserId))
            .Select(up => new
            {
                up.UserId,
                up.Goal,
                up.Experience,
                PhotoFileId = up.Photo != null ? up.Photo.FileId : null
            })
            .ToDictionaryAsync(k => k.UserId, cancellationToken);

        var responseItems = new List<CoachStudentSummaryResponse>();
        foreach (var user in items)
        {
            var profile = userProfiles.TryGetValue(user.Id, out var up) ? up : null;

            responseItems.Add(new CoachStudentSummaryResponse(
                user.Id,
                user.DisplayName ?? $"{user.FirstName} {user.LastName}",
                profile?.PhotoFileId,
                profile?.Goal,
                profile?.Experience,
                default,
                CoachPermissions.None,
                0));
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
