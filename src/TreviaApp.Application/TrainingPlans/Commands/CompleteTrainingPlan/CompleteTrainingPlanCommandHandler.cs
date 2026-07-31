namespace TreviaApp.Application.TrainingPlans.Commands.CompleteTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class CompleteTrainingPlanCommandHandler : ICommandHandler<CompleteTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CompleteTrainingPlanCommandHandler> _logger;

    public CompleteTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CompleteTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(CompleteTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!CanManagePlanStatus(tp, userId))
            throw new DomainException("Você não tem permissão para marcar este plano de treino como concluído.", ErrorCodes.TrainingPlanNotOwner);

        tp.Complete();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("CompleteTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanCompleted TrainingPlanId={TrainingPlanId} UserId={UserId}",
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
