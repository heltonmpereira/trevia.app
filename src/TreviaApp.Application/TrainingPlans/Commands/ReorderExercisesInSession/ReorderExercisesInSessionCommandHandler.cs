namespace TreviaApp.Application.TrainingPlans.Commands.ReorderExercisesInSession;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class ReorderExercisesInSessionCommandHandler : ICommandHandler<ReorderExercisesInSessionCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ReorderExercisesInSessionCommandHandler> _logger;

    public ReorderExercisesInSessionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<ReorderExercisesInSessionCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(ReorderExercisesInSessionCommand request, CancellationToken cancellationToken)
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

        var dict = new Dictionary<Guid, int>();
        foreach (var o in request.Orders)
            dict[o.SessionExerciseId] = o.NewOrder;

        tp.ReorderExercisesInSession(request.SessionId, dict);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("ReorderExercisesInSessionHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId} SessionId={SessionId}", tp.Id, request.SessionId);

        _logger.LogInformation(
            "ExercisesInSessionReordered TrainingPlanId={TrainingPlanId} SessionId={SessionId} UserId={UserId}",
            tp.Id,
            request.SessionId,
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
