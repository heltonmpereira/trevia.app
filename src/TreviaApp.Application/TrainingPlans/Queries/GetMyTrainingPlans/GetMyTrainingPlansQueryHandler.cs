namespace TreviaApp.Application.TrainingPlans.Queries.GetMyTrainingPlans;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;
using TrainingPlanStatus = TreviaApp.Shared.Enums.TrainingPlanStatus;

public sealed class GetMyTrainingPlansQueryHandler : IQueryHandler<GetMyTrainingPlansQuery, TrainingPlansSearchPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetMyTrainingPlansQueryHandler> _logger;

    public GetMyTrainingPlansQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetMyTrainingPlansQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlansSearchPagedResponse> Handle(GetMyTrainingPlansQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        IQueryable<TrainingPlan> query;

        var isStudent = _currentUser.IsInRole(AppRoles.Student);
        var isTrainerOrStaff = _currentUser.IsInRole(AppRoles.Trainer)
                               || _currentUser.IsInRole(AppRoles.Administrator)
                               || _currentUser.IsInRole(AppRoles.GymManager);

        if (isStudent && !isTrainerOrStaff)
        {
            query = _db.Set<TrainingPlan>()
                .Include(tp => tp.Sessions)
                .ThenInclude(s => s.Exercises)
                .Where(tp => tp.AssignedToStudentId == userId);
        }
        else
        {
            query = _db.Set<TrainingPlan>()
                .Include(tp => tp.Sessions)
                .ThenInclude(s => s.Exercises)
                .Where(tp => tp.CreatedByUserId == userId);
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(tp => tp.Status == request.StatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var search = $"%{request.SearchName}%";
            query = query.Where(tp => EF.Functions.Like(tp.Name, search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        IOrderedQueryable<TrainingPlan> ordered;
        switch ((request.SortBy ?? "createdAtDesc").ToLowerInvariant())
        {
            case "createdatasc":
                ordered = query.OrderBy(tp => tp.CreatedAt);
                break;
            case "nameasc":
                ordered = query.OrderBy(tp => tp.Name);
                break;
            case "namedesc":
                ordered = query.OrderByDescending(tp => tp.Name);
                break;
            case "createdatdesc":
            default:
                ordered = query.OrderByDescending(tp => tp.CreatedAt);
                break;
        }

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var summaryItems = new List<TrainingPlanSummaryResponse>();
        foreach (var tp in items)
        {
            int totalSessions = tp.Sessions.Count;
            int totalExercises = tp.Sessions.Sum(s => s.Exercises.Count);
            summaryItems.Add(TrainingPlanMappings.MapToSummary(tp, totalSessions, totalExercises, null, null));
        }

        return new TrainingPlansSearchPagedResponse(totalCount, page, pageSize, totalPages, summaryItems);
    }
}
