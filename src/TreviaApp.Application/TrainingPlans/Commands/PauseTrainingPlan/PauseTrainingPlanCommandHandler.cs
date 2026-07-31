namespace TreviaApp.Application.TrainingPlans.Commands.PauseTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class PauseTrainingPlanCommandHandler : ICommandHandler<PauseTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PauseTrainingPlanCommandHandler> _logger;

    public PauseTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<PauseTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(PauseTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!CanManagePlanStatus(tp, userId))
            throw new DomainException("Você não tem permissão para pausar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        tp.Pause();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("PauseTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanPaused TrainingPlanId={TrainingPlanId} UserId={UserId}",
            tp.Id,
            userId);

        return TrainingPlanMappings.MapToDetail(tp, null, null, false);
    }

    private bool CanManagePlanStatus(TrainingPlan tp, Guid? userId)
    {
        var isOwnerOrStaff = (userId.HasValue && userId == tp.CreatedByUserId)
                             || _currentUser.IsInRole(AppRoles.Administrator)
                             || _currentUser.IsInRole(AppRoles.GymManager);

        var isAssignedStudent = userId.HasValue && userId == tp.AssignedToStudentId;

        return isOwnerOrStaff || isAssignedStudent;
    }
}
