namespace TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingSession;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class UpdateTrainingSessionCommandHandler : ICommandHandler<UpdateTrainingSessionCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateTrainingSessionCommandHandler> _logger;

    public UpdateTrainingSessionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpdateTrainingSessionCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(UpdateTrainingSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdminOrGymManager(tp, userId))
            throw new DomainException("Você não tem permissão para editar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        var session = tp.Sessions.FirstOrDefault(s => s.Id == request.SessionId);
        if (session is null)
            throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.TrainingPlanSessionNotFound);

        tp.UpdateSession(
            request.SessionId,
            request.Name,
            request.Description,
            request.Order,
            request.SuggestedDayOfWeek,
            request.EstimatedDuration,
            request.CoachNotesInternal,
            request.Focus);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpdateTrainingSessionHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId} SessionId={SessionId}", tp.Id, request.SessionId);

        _logger.LogInformation(
            "TrainingSessionUpdated TrainingPlanId={TrainingPlanId} SessionId={SessionId} UserId={UserId}",
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
