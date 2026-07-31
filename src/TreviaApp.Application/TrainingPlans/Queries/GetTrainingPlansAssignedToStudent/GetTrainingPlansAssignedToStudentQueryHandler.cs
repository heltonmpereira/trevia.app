namespace TreviaApp.Application.TrainingPlans.Queries.GetTrainingPlansAssignedToStudent;

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

public sealed class GetTrainingPlansAssignedToStudentQueryHandler : IQueryHandler<GetTrainingPlansAssignedToStudentQuery, TrainingPlansSearchPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetTrainingPlansAssignedToStudentQueryHandler> _logger;

    public GetTrainingPlansAssignedToStudentQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetTrainingPlansAssignedToStudentQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlansSearchPagedResponse> Handle(GetTrainingPlansAssignedToStudentQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var isStaff = _currentUser.IsInRole(AppRoles.Administrator)
                      || _currentUser.IsInRole(AppRoles.GymManager);

        var isOwnStudent = userId == request.StudentId;

        if (!isStaff && !isOwnStudent)
        {
            throw new DomainException(
                "Você não tem permissão para visualizar os planos de treino deste aluno.",
                ErrorCodes.Forbidden);
        }

        var query = _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .Where(tp => tp.AssignedToStudentId == request.StudentId);

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(tp => tp.Status == request.StatusFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(tp => tp.CreatedAt)
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
