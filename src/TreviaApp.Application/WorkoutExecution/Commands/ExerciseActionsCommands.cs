using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;
using TreviaApp.Shared.Errors;
using TreviaApp.Application.WorkoutExecution.Commands;
using static TreviaApp.Application.WorkoutExecution.Commands.PauseResumeFinish;

namespace TreviaApp.Application.WorkoutExecution.Commands;

public static class ExerciseActions
{
    public sealed record SkipWorkoutExerciseCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            Guid WorkoutExerciseId,
            string? SkipReason = null)
        : IRequest<Result<WorkoutExerciseResponse>>;

    public sealed record AddExtraSetToExerciseCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            Guid WorkoutExerciseId,
            int? SuggestedSetNumber = null)
        : IRequest<Result<WorkoutSetResponse>>;

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
        : IRequest<Result<WorkoutSetResponse>>;

    public sealed class SkipWorkoutExerciseCommandHandler : IRequestHandler<SkipWorkoutExerciseCommand, Result<WorkoutExerciseResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<SkipWorkoutExerciseCommandHandler> _logger;

        public SkipWorkoutExerciseCommandHandler(IApplicationDbContext db, ILogger<SkipWorkoutExerciseCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<WorkoutExerciseResponse>> Handle(SkipWorkoutExerciseCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutExerciseResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != request.CurrentUserId)
                return Result.Failure<WorkoutExerciseResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                return Result.Failure<WorkoutExerciseResponse>(Error.Failure(ErrorCodes.WorkoutNotInProgressOrPaused));

            var wex = ws.FindExercise(request.WorkoutExerciseId);
            if (wex == null) return Result.Failure<WorkoutExerciseResponse>(Error.NotFound(ErrorCodes.WorkoutExerciseNotFound));
            if (wex.IsSkipped)
                return Result.Failure<WorkoutExerciseResponse>(Error.Failure(ErrorCodes.WorkoutExerciseAlreadySkipped));

            wex.Skip(request.SkipReason);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: SkipWorkoutExercise Ws={Ws} Wex={Wex}.", ws.Id, wex.Id);

            var ex = await _db.Set<TreviaApp.Domain.Exercises.Exercise>()
                .FirstOrDefaultAsync(e => e.Id == wex.ExerciseId, ct);
            return Result.Success(MapExercise(wex, ex?.Name));
        }
    }

    public sealed class AddExtraSetToExerciseCommandHandler : IRequestHandler<AddExtraSetToExerciseCommand, Result<WorkoutSetResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<AddExtraSetToExerciseCommandHandler> _logger;

        public AddExtraSetToExerciseCommandHandler(IApplicationDbContext db, ILogger<AddExtraSetToExerciseCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<WorkoutSetResponse>> Handle(AddExtraSetToExerciseCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutSetResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != request.CurrentUserId)
                return Result.Failure<WorkoutSetResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                return Result.Failure<WorkoutSetResponse>(Error.Failure(ErrorCodes.WorkoutNotInProgressOrPaused));

            var wex = ws.FindExercise(request.WorkoutExerciseId);
            if (wex == null) return Result.Failure<WorkoutSetResponse>(Error.NotFound(ErrorCodes.WorkoutExerciseNotFound));

            var extra = wex.AddExtraSet(request.SuggestedSetNumber ?? 0);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: AddExtraSet Ws={Ws} SetId={SetId}.", ws.Id, extra.Id);
            return Result.Success(MapSet(extra));
        }
    }

    public sealed class LogWorkoutSetCommandHandler : IRequestHandler<LogWorkoutSetCommand, Result<WorkoutSetResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<LogWorkoutSetCommandHandler> _logger;

        public LogWorkoutSetCommandHandler(IApplicationDbContext db, ILogger<LogWorkoutSetCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<WorkoutSetResponse>> Handle(LogWorkoutSetCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutSetResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != request.CurrentUserId)
                return Result.Failure<WorkoutSetResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                return Result.Failure<WorkoutSetResponse>(Error.Failure(ErrorCodes.WorkoutNotInProgressOrPaused));

            var wex = ws.FindExercise(request.WorkoutExerciseId);
            if (wex == null) return Result.Failure<WorkoutSetResponse>(Error.NotFound(ErrorCodes.WorkoutExerciseNotFound));

            var set = wex.FindSet(request.WorkoutSetId);
            if (set == null) return Result.Failure<WorkoutSetResponse>(Error.NotFound(ErrorCodes.WorkoutSetNotFound));

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
                return Result.Failure<WorkoutSetResponse>(Error.Failure(ErrorCodes.WorkoutSetAlreadyLogged, ex.Message));
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: LogWorkoutSet SetId={SetId}.", set.Id);
            return Result.Success(MapSet(set));
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
