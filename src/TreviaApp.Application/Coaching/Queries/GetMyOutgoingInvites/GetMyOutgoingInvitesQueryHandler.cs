namespace TreviaApp.Application.Coaching.Queries.GetMyOutgoingInvites;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Enums;

public sealed class GetMyOutgoingInvitesQueryHandler : IQueryHandler<GetMyOutgoingInvitesQuery, CoachingInvitesPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetMyOutgoingInvitesQueryHandler> _logger;

    public GetMyOutgoingInvitesQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetMyOutgoingInvitesQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachingInvitesPagedResponse> Handle(GetMyOutgoingInvitesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var query = _db.Set<CoachStudentRequest>()
            .Include(r => r.Coach)
            .Include(r => r.Student)
            .Where(r =>
                (r.Direction == CoachInviteDirection.CoachToStudent && r.CoachId == userId) ||
                (r.Direction == CoachInviteDirection.StudentToCoach && r.StudentId == userId));

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(r => r.Status == request.StatusFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        IOrderedQueryable<CoachStudentRequest> ordered;
        switch ((request.SortBy ?? "createdAtDesc").ToLowerInvariant())
        {
            case "createdatasc":
                ordered = query.OrderBy(r => r.CreatedAt);
                break;
            case "createdatdesc":
            default:
                ordered = query.OrderByDescending(r => r.CreatedAt);
                break;
        }

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var coachUserIds = items.Select(r => r.CoachId).Distinct().ToList();
        var studentUserIds = items.Select(r => r.StudentId).Distinct().ToList();
        var allUserIds = coachUserIds.Concat(studentUserIds).Distinct().ToList();

        var profilePhotos = await _db.Set<UserProfile>()
            .Where(up => allUserIds.Contains(up.UserId))
            .Select(up => new { up.UserId, up.Photo!.FileId })
            .ToDictionaryAsync(k => k.UserId, v => v.FileId, cancellationToken);

        var responseItems = new List<CoachInviteResponse>();
        foreach (var r in items)
        {
            string? coachPhotoFileId = profilePhotos.TryGetValue(r.CoachId, out var cp) ? cp : null;
            string? studentPhotoFileId = profilePhotos.TryGetValue(r.StudentId, out var sp) ? sp : null;

            responseItems.Add(new CoachInviteResponse(
                r.Id,
                r.CoachId,
                $"{r.Coach.FirstName} {r.Coach.LastName}",
                coachPhotoFileId,
                r.StudentId,
                $"{r.Student.FirstName} {r.Student.LastName}",
                studentPhotoFileId,
                r.Direction,
                r.Status,
                r.Message,
                r.ExpiresAt,
                r.IsExpired,
                r.CreatedAt,
                r.RespondedAt,
                r.GrantedPermissionsOnAccept,
                r.RejectionReason));
        }

        return new CoachingInvitesPagedResponse
        {
            Items = responseItems,
            TotalCount = totalCount,
            PageIndex = page,
            PageSize = pageSize,
            HasNextPage = (page * pageSize) < totalCount
        };
    }
}
