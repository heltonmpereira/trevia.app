using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.WorkoutExecution.Mappings;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Application.WorkoutExecution.Commands.StartWorkoutSession;

public sealed class StartWorkoutSessionCommandHandler : IRequestHandler<StartWorkoutSessionCommand, WorkoutSessionSummaryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<StartWorkoutSessionCommandHandler> _logger;

    public StartWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<StartWorkoutSessionCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<WorkoutSessionSummaryResponse> Handle(StartWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.WeekNumberInPlan < 1)
            throw new DomainException("Número da semana inválido.", ErrorCodes.WorkoutWeekNumberInvalid);

        var sessionPrescription = await _db.Set<TrainingSession>()
            .Include(s => s.TrainingPlan)
            .Include(s => s.Exercises).ThenInclude(e => e.Prescriptions)
            .FirstOrDefaultAsync(s => s.Id == request.TrainingSessionId, cancellationToken);

        if (sessionPrescription == null)
            throw new DomainException("Sessão de treino (prescrição) não encontrada.", ErrorCodes.WorkoutTrainingSessionNotFound);

        var plan = sessionPrescription.TrainingPlan;
        if (plan != null && plan.AssignedToStudentId.HasValue && plan.AssignedToStudentId.Value != request.CurrentUserId)
            throw new DomainException("Ficha não atribuída a este aluno.", ErrorCodes.WorkoutTrainingPlanNotAssignedToStudent);

        if (plan != null && plan.AssignedToStudentId is null && plan.CreatedByUserId != request.CurrentUserId)
            throw new DomainException("Apenas o dono da ficha pode iniciar uma sessão.", ErrorCodes.WorkoutCannotStartNotOwner);

        var alreadyActive = await _db.Set<WorkoutSession>()
            .AnyAsync(s => s.StudentId == request.CurrentUserId
                           && (s.Status == WorkoutStatus.InProgress || s.Status == WorkoutStatus.Paused),
                cancellationToken);
        if (alreadyActive)
            throw new DomainException("Você já possui uma sessão de treino ativa. Finalize-a antes de iniciar outra.", ErrorCodes.WorkoutAlreadyHasActiveSession);

        var wk = new WorkoutSession(
            request.CurrentUserId,
            plan?.Id,
            sessionPrescription.Id,
            sessionPrescription.Name,
            request.WeekNumberInPlan);

        wk.Start();

        var exercises = sessionPrescription.Exercises
            .OrderBy(e => e.Order)
            .Select(e => (e.Id, e.ExerciseId, e.Order, e.NotesForStudent))
            .ToList();
        wk.AddExercisesFromPrescription(exercises);

        _db.Set<WorkoutSession>().Add(wk);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Criação explícita concluída: WorkoutSession {Id} para TrainingSession {TsId}.", wk.Id, sessionPrescription.Id);

        var wexPrescriptions = sessionPrescription.Exercises.ToDictionary(e => e.Id);

        foreach (var wExercise in wk.Exercises)
        {
            if (!wExercise.SessionExerciseId.HasValue || !wexPrescriptions.TryGetValue(wExercise.SessionExerciseId.Value, out var se))
                continue;
            foreach (var p in se.Prescriptions.OrderBy(p => p.SetNumber))
            {
                wExercise.AddSetFromPrescription(
                    p.Id,
                    p.SetNumber,
                    p.TargetRepsMin,
                    p.TargetRepsMax,
                    p.LoadValue,
                    p.LoadUnit,
                    p.RestAfterSeconds,
                    p.TechniqueApplied);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Atualização de séries de prescrição concluída para WorkoutSession {Id}.", wk.Id);

        var agg = wk.AggregateWorkoutSessionTotals();
        return new WorkoutSessionSummaryResponse(
            wk.Id,
            wk.TrainingPlanId,
            plan?.Name,
            wk.TrainingSessionId,
            wk.Name,
            wk.Status,
            wk.StartedAt,
            wk.FinishedAt,
            agg.seconds,
            agg.activeSeconds,
            wk.OverallRating,
            wk.WeekNumberInPlan,
            agg.excCount,
            agg.completedSets,
            agg.totalVolume);
    }
}
