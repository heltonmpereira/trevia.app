namespace TreviaApp.Application.TrainingPlans.Queries.GetTrainingPlanById;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public sealed class GetTrainingPlanByIdQueryHandler : IQueryHandler<GetTrainingPlanByIdQuery, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetTrainingPlanByIdQueryHandler> _logger;

    public GetTrainingPlanByIdQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetTrainingPlanByIdQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(GetTrainingPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var tp = await _db.Set<TrainingPlan>()
            .AsNoTracking()
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .ThenInclude(e => e.Exercise)
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .ThenInclude(e => e.Prescriptions)
            .Include(tp => tp.CreatedByUser)
            .Include(tp => tp.AssignedToStudent)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        var userId = _currentUser.UserId;
        var isOwnerOrStaff = (userId.HasValue && userId == tp.CreatedByUserId)
                             || _currentUser.IsInRole(AppRoles.Administrator)
                             || _currentUser.IsInRole(AppRoles.GymManager);

        var isAssignedStudent = userId.HasValue && userId == tp.AssignedToStudentId;

        if (!tp.IsPublicTemplate)
        {
            if (tp.Visibility == Visibility.Private)
            {
                if (!isOwnerOrStaff && !isAssignedStudent)
                {
                    throw new DomainException(
                        "Você não tem permissão para visualizar este plano de treino.",
                        ErrorCodes.Forbidden);
                }
            }
        }

        var createdByName = tp.CreatedByUser?.DisplayName ?? tp.CreatedByUser?.UserName ?? tp.CreatedByUser?.Email;
        var assignedToStudentName = tp.AssignedToStudent?.DisplayName ?? tp.AssignedToStudent?.UserName ?? tp.AssignedToStudent?.Email;

        bool hideCoachNotes = false;
        bool isOwner = userId.HasValue && userId == tp.CreatedByUserId;
        bool isAdminOrGymManager = _currentUser.IsInRole(AppRoles.Administrator) || _currentUser.IsInRole(AppRoles.GymManager);

        if ((tp.AssignedToStudentId ?? Guid.Empty) == (userId ?? Guid.Empty) && !isOwner && !isAdminOrGymManager)
        {
            hideCoachNotes = true;
        }

        return TrainingPlanMappings.MapToDetail(tp, createdByName, assignedToStudentName, hideCoachNotes);
    }
}
