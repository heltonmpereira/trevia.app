namespace TreviaApp.Application.TrainingPlans.Commands.PublishTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public sealed class PublishTrainingPlanCommandHandler : ICommandHandler<PublishTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PublishTrainingPlanCommandHandler> _logger;

    public PublishTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<PublishTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(PublishTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdminOrGymManager(tp, userId))
            throw new DomainException("Você não tem permissão para publicar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        if (tp.Sessions.Count == 0 || !tp.Sessions.Any(s => s.Exercises.Count > 0))
            throw new DomainException("O plano de treino precisa ter ao menos uma sessão com exercícios para ser publicado.", ErrorCodes.TrainingPlanNotPublishable);

        if (request.AsPublicTemplate)
        {
            var allExerciseIds = tp.Sessions
                .SelectMany(s => s.Exercises)
                .Select(e => e.ExerciseId)
                .Distinct()
                .ToList();

            var exercises = await _db.Set<Exercise>()
                .Where(e => allExerciseIds.Contains(e.Id))
                .ToListAsync(cancellationToken);

            foreach (var exId in allExerciseIds)
            {
                var ex = exercises.FirstOrDefault(e => e.Id == exId);
                if (ex == null || ex.Status != ExerciseStatus.Approved)
                {
                    var exName = ex?.Name ?? $"Exercício não encontrado (Id: {exId})";
                    throw new DomainException(
                        $"Não é possível publicar como template público: o exercício \"{exName}\" não está aprovado globalmente.",
                        ErrorCodes.ExerciseNotApprovedGlobal);
                }
            }
        }

        tp.Publish();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("PublishTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanPublished TrainingPlanId={TrainingPlanId} UserId={UserId} AsPublicTemplate={AsPublicTemplate}",
            tp.Id,
            userId,
            request.AsPublicTemplate);

        return TrainingPlanMappings.MapToDetail(tp, null, null, false);
    }

    private bool IsOwnerOrAdminOrGymManager(TrainingPlan tp, Guid? userId)
    {
        return (userId.HasValue && userId == tp.CreatedByUserId)
               || _currentUser.IsInRole(AppRoles.Administrator)
               || _currentUser.IsInRole(AppRoles.GymManager);
    }
}
