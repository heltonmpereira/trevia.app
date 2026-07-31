namespace TreviaApp.Application.TrainingPlans.Commands.AddSessionToTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class AddSessionToTrainingPlanCommandHandler : ICommandHandler<AddSessionToTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AddSessionToTrainingPlanCommandHandler> _logger;

    public AddSessionToTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<AddSessionToTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(AddSessionToTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdminOrGymManager(tp, userId))
            throw new DomainException("Você não tem permissão para editar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        tp.AddSession(
            request.Name,
            request.Description,
            request.Order,
            request.SuggestedDayOfWeek,
            request.EstimatedDuration);

        if (!string.IsNullOrWhiteSpace(request.CoachNotesInternal) || !string.IsNullOrWhiteSpace(request.Focus))
        {
            var session = tp.Sessions.OrderByDescending(s => s.Order).FirstOrDefault();
            if (session != null)
            {
                tp.UpdateSession(
                    session.Id,
                    session.Name,
                    session.Description,
                    session.Order,
                    session.SuggestedDayOfWeek,
                    session.EstimatedDurationMin,
                    request.CoachNotesInternal,
                    request.Focus);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AddSessionToTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingSessionAdded TrainingPlanId={TrainingPlanId} UserId={UserId} SessionName={Name}",
            tp.Id,
            userId,
            request.Name);

        return TrainingPlanMappings.MapToDetail(tp, null, null, false);
    }

    private bool IsOwnerOrAdminOrGymManager(TrainingPlan tp, Guid? userId)
    {
        return (userId.HasValue && userId == tp.CreatedByUserId)
               || _currentUser.IsInRole(AppRoles.Administrator)
               || _currentUser.IsInRole(AppRoles.GymManager);
    }
}
