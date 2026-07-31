namespace TreviaApp.Application.TrainingPlans.Commands.AddExerciseToTrainingSession;

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

public sealed class AddExerciseToTrainingSessionCommandHandler : ICommandHandler<AddExerciseToTrainingSessionCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AddExerciseToTrainingSessionCommandHandler> _logger;

    public AddExerciseToTrainingSessionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<AddExerciseToTrainingSessionCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(AddExerciseToTrainingSessionCommand request, CancellationToken cancellationToken)
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

        var exercise = await _db.Set<Exercise>()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (tp.IsPublicTemplate || tp.Status == TrainingPlanStatus.Published)
        {
            if (exercise.Status != ExerciseStatus.Approved)
                throw new DomainException(
                    $"Não é possível adicionar exercício não aprovado \"{exercise.Name}\" em um plano publicado/template público.",
                    ErrorCodes.ExerciseNotApprovedGlobal);
        }

        tp.AddExerciseToSession(
            request.SessionId,
            request.ExerciseId,
            request.Order,
            request.NotesForStudent);

        var session = tp.Sessions.FirstOrDefault(s => s.Id == request.SessionId);
        if (session is null)
            throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.TrainingPlanSessionNotFound);

        var sessionExercise = session.Exercises
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault(e => e.ExerciseId == request.ExerciseId && e.Order == request.Order);

        if (sessionExercise is null)
            sessionExercise = session.Exercises.OrderByDescending(e => e.CreatedAt).FirstOrDefault();

        if (sessionExercise != null)
        {
            if (!string.IsNullOrWhiteSpace(request.NotesForCoach) || request.DefaultRestBetweenSetsSeconds.HasValue)
            {
                sessionExercise.UpdateBasicInfo(
                    sessionExercise.Order,
                    sessionExercise.NotesForStudent,
                    request.NotesForCoach,
                    request.DefaultRestBetweenSetsSeconds ?? sessionExercise.RestBetweenSetsSeconds,
                    sessionExercise.GlobalSetTechniqueAppliedToAllSets,
                    sessionExercise.GlobalLoadOverrideKg,
                    sessionExercise.GlobalRepsOverride);
            }

            if (request.InitialSets != null && request.InitialSets.Count > 0)
            {
                foreach (var set in request.InitialSets.OrderBy(s => s.SetNumber))
                {
                    int? tutSeconds = set.TUTSeconds.HasValue ? (int?)set.TUTSeconds.Value.TotalSeconds : null;
                    sessionExercise.AddPrescriptionSet(
                        set.SetNumber,
                        set.TargetRepsMin,
                        set.TargetRepsMax,
                        set.LoadValue,
                        set.LoadUnit,
                        set.RestAfterSeconds,
                        set.TechniqueApplied,
                        tutSeconds,
                        set.NotesProfessor);
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AddExerciseToTrainingSessionHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId} SessionId={SessionId}", tp.Id, request.SessionId);

        _logger.LogInformation(
            "ExerciseAddedToSession TrainingPlanId={TrainingPlanId} SessionId={SessionId} ExerciseId={ExerciseId} UserId={UserId}",
            tp.Id,
            request.SessionId,
            request.ExerciseId,
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
