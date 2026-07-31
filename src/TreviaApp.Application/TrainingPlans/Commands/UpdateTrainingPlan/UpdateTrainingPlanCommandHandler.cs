namespace TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class UpdateTrainingPlanCommandHandler : ICommandHandler<UpdateTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateTrainingPlanCommandHandler> _logger;

    public UpdateTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpdateTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(UpdateTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .ThenInclude(e => e.Prescriptions)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdminOrGymManager(tp, userId))
            throw new DomainException("Você não tem permissão para editar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        tp.UpdateBasicInfo(
            request.Name,
            request.Description,
            request.InstructionsIntro,
            request.NotesForStudent,
            request.Tags,
            request.SplitType,
            request.Visibility,
            request.TotalWeeks,
            request.SessionsPerWeek);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpdateTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanUpdated TrainingPlanId={TrainingPlanId} UserId={UserId}",
            tp.Id,
            userId);

        return TrainingPlanMappings.MapToDetail(tp, null, null, false);
    }

    private bool IsOwnerOrAdminOrGymManager(TrainingPlan tp, Guid? userId)
    {
        return (userId.HasValue && userId == tp.CreatedByUserId)
               || _currentUser.IsInRole(AppRoles.Administrator)
               || _currentUser.IsInRole(AppRoles.GymManager);
    }
}
