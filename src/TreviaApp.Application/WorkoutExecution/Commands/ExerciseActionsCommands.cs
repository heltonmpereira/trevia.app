using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;
using static TreviaApp.Application.WorkoutExecution.Commands.PauseResumeFinish;

namespace TreviaApp.Application.WorkoutExecution.Commands;

public static class ExerciseActions
{
    public sealed record SkipWorkoutExerciseCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            Guid WorkoutExerciseId,
            string? SkipReason = null)
        : ICommand<WorkoutExerciseResponse>;

    public sealed record AddExtraSetToExerciseCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            Guid WorkoutExerciseId,
            int? SuggestedSetNumber = null)
        : ICommand<WorkoutSetResponse>;

    public sealed record LogWorkoutSetCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            Guid WorkoutExerciseId,
            Guid WorkoutSetId,
            int? ActualReps = null,
            decimal? ActualLoadValue = null,
            PrescriptionLoadUnit? ActualLoadUnit = null,
            long? ActualDurationSeconds = null,
            decimal? DistanceKm = null,
            decimal? SpeedKmh = null,
            decimal? InclinePercent = null,
            int? Calories = null,
            bool Completed = true,
            SetRating? DifficultyRating = null,
            string? Notes = null)
        : ICommand<WorkoutSetResponse>;

    public sealed class SkipWorkoutExerciseCommandHandler : IRequestHandler<SkipWorkoutExerciseCommand, WorkoutExerciseResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<SkipWorkoutExerciseCommandHandler> _logger;

        public SkipWorkoutExerciseCommandHandler(IApplicationDbContext db, ILogger<SkipWorkoutExerciseCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkoutExerciseResponse> Handle(SkipWorkoutExerciseCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != request.CurrentUserId)
                throw new DomainException("Apenas o dono da sessão pode pular exercícios.", ErrorCodes.WorkoutCannotStartNotOwner);
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                throw new DomainException("Você só pode pular exercícios durante o treino.", ErrorCodes.WorkoutNotInProgressOrPaused);

            var wex = ws.FindExercise(request.WorkoutExerciseId);
            if (wex == null) throw new DomainException("Exercício da sessão não encontrado.", ErrorCodes.WorkoutExerciseNotFound);
            if (wex.IsSkipped)
                throw new DomainException("Este exercício já foi marcado como pulado.", ErrorCodes.WorkoutExerciseAlreadySkipped);

            wex.Skip(request.SkipReason);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: SkipWorkoutExercise Ws={Ws} Wex={Wex}.", ws.Id, wex.Id);

            var ex = await _db.Set<TreviaApp.Domain.Exercises.Exercise>()
                .FirstOrDefaultAsync(e => e.Id == wex.ExerciseId, ct);
            return MapExercise(wex, ex?.Name);
        }
    }

    public sealed class AddExtraSetToExerciseCommandHandler : IRequestHandler<AddExtraSetToExerciseCommand, WorkoutSetResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<AddExtraSetToExerciseCommandHandler> _logger;

        public AddExtraSetToExerciseCommandHandler(IApplicationDbContext db, ILogger<AddExtraSetToExerciseCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkoutSetResponse> Handle(AddExtraSetToExerciseCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != request.CurrentUserId)
                throw new DomainException("Apenas o dono da sessão pode adicionar séries.", ErrorCodes.WorkoutCannotStartNotOwner);
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                throw new DomainException("Você só pode adicionar séries durante o treino.", ErrorCodes.WorkoutNotInProgressOrPaused);

            var wex = ws.FindExercise(request.WorkoutExerciseId);
            if (wex == null) throw new DomainException("Exercício da sessão não encontrado.", ErrorCodes.WorkoutExerciseNotFound);

            var extra = wex.AddExtraSet(request.SuggestedSetNumber ?? 0);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: AddExtraSet Ws={Ws} SetId={SetId}.", ws.Id, extra.Id);
            return MapSet(extra);
        }
    }

    public sealed class LogWorkoutSetCommandHandler : IRequestHandler<LogWorkoutSetCommand, WorkoutSetResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<LogWorkoutSetCommandHandler> _logger;

        public LogWorkoutSetCommandHandler(IApplicationDbContext db, ILogger<LogWorkoutSetCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkoutSetResponse> Handle(LogWorkoutSetCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != request.CurrentUserId)
                throw new DomainException("Apenas o dono da sessão pode registrar séries.", ErrorCodes.WorkoutCannotStartNotOwner);
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                throw new DomainException("Você só pode registrar séries durante o treino.", ErrorCodes.WorkoutNotInProgressOrPaused);

            var wex = ws.FindExercise(request.WorkoutExerciseId);
            if (wex == null) throw new DomainException("Exercício da sessão não encontrado.", ErrorCodes.WorkoutExerciseNotFound);

            var set = wex.FindSet(request.WorkoutSetId);
            if (set == null) throw new DomainException("Série não encontrada no exercício.", ErrorCodes.WorkoutSetNotFound);

            TimeSpan? duration = request.ActualDurationSeconds.HasValue
                ? TimeSpan.FromSeconds(request.ActualDurationSeconds.Value)
                : null;

            try
            {
                set.LogExecution(
                    request.ActualReps,
                    request.ActualLoadValue,
                    request.ActualLoadUnit,
                    duration,
                    request.DistanceKm,
                    request.SpeedKmh,
                    request.InclinePercent,
                    request.Calories,
                    request.Completed,
                    request.DifficultyRating,
                    request.Notes);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException("Dados inválidos para a série.", ErrorCodes.WorkoutSetAlreadyLogged, ex.Message);
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: LogWorkoutSet SetId={SetId}.", set.Id);
            return MapSet(set);
        }
    }

    public static WorkoutExerciseResponse MapExercise(WorkoutExercise wex, string? exerciseName)
    {
        return new WorkoutExerciseResponse(
            wex.Id,
            wex.SessionExerciseId,
            wex.ExerciseId,
            exerciseName ?? $"Exercise_{wex.ExerciseId}",
            wex.Order,
            wex.IsSkipped,
            wex.SkipReason,
            wex.Notes,
            wex.Sets.OrderBy(s => s.SetNumber).Select(MapSet).ToList());
    }

    public static WorkoutSetResponse MapSet(WorkoutSet s)
    {
        return new WorkoutSetResponse(
            s.Id,
            s.SetPrescriptionId,
            s.SetNumber,
            s.TargetRepsMin,
            s.TargetRepsMax,
            s.TargetLoadValue,
            s.TargetLoadUnit,
            s.ActualReps,
            s.ActualLoadValue,
            s.ActualLoadUnit,
            s.ActualDuration.HasValue ? (long?)s.ActualDuration.Value.TotalSeconds : null,
            s.DistanceKm,
            s.SpeedKmh,
            s.InclinePercent,
            s.Calories,
            s.Completed,
            s.DifficultyRating,
            s.Notes,
            s.IsAdditionalSet,
            s.VolumeKg);
    }
}
