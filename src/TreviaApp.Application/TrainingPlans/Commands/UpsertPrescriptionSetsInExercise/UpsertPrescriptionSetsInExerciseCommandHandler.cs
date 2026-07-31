namespace TreviaApp.Application.TrainingPlans.Commands.UpsertPrescriptionSetsInExercise;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class UpsertPrescriptionSetsInExerciseCommandHandler : ICommandHandler<UpsertPrescriptionSetsInExerciseCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpsertPrescriptionSetsInExerciseCommandHandler> _logger;

    public UpsertPrescriptionSetsInExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpsertPrescriptionSetsInExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(UpsertPrescriptionSetsInExerciseCommand request, CancellationToken cancellationToken)
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

        var session = tp.Sessions.FirstOrDefault(s => s.Id == request.SessionId);
        if (session is null)
            throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.TrainingPlanSessionNotFound);

        var sessionExercise = session.Exercises.FirstOrDefault(e => e.Id == request.SessionExerciseId);
        if (sessionExercise is null)
            throw new DomainException("Exercício da sessão não encontrado.", ErrorCodes.TrainingPlanSessionExerciseNotFound);

        var existingSets = sessionExercise.Prescriptions.ToList();
        foreach (var set in existingSets)
        {
            sessionExercise.RemovePrescriptionSet(set.Id);
        }

        foreach (var set in request.Sets.OrderBy(s => s.SetNumber))
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

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpsertPrescriptionSetsInExerciseHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId} SessionId={SessionId} SessionExerciseId={SessionExerciseId}", tp.Id, request.SessionId, request.SessionExerciseId);

        _logger.LogInformation(
            "PrescriptionSetsUpserted TrainingPlanId={TrainingPlanId} SessionId={SessionId} SessionExerciseId={SessionExerciseId} SetsCount={SetsCount} UserId={UserId}",
            tp.Id,
            request.SessionId,
            request.SessionExerciseId,
            request.Sets.Count,
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
