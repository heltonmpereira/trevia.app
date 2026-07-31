namespace TreviaApp.Application.TrainingPlans.Commands.ArchiveTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class ArchiveTrainingPlanCommandHandler : ICommandHandler<ArchiveTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ArchiveTrainingPlanCommandHandler> _logger;

    public ArchiveTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<ArchiveTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(ArchiveTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdminOrGymManager(tp, userId))
            throw new DomainException("Você não tem permissão para arquivar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        tp.Archive();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("ArchiveTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanArchived TrainingPlanId={TrainingPlanId} UserId={UserId}",
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
