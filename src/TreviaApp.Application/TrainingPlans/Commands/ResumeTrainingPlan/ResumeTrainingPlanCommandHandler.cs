namespace TreviaApp.Application.TrainingPlans.Commands.ResumeTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class ResumeTrainingPlanCommandHandler : ICommandHandler<ResumeTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ResumeTrainingPlanCommandHandler> _logger;

    public ResumeTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<ResumeTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(ResumeTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!CanManagePlanStatus(tp, userId))
            throw new DomainException("Você não tem permissão para retomar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        tp.Resume();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("ResumeTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanResumed TrainingPlanId={TrainingPlanId} UserId={UserId}",
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
