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
using TreviaApp.Application.WorkoutExecution.Mappings;

namespace TreviaApp.Application.WorkoutExecution.Commands;

public static class PauseResumeFinish
{
    public sealed record PauseWorkoutSessionCommand(Guid CurrentUserId, Guid WorkoutSessionId)
        : IRequest<Result<WorkoutSessionSummaryResponse>>;

    public sealed record ResumeWorkoutSessionCommand(Guid CurrentUserId, Guid WorkoutSessionId)
        : IRequest<Result<WorkoutSessionSummaryResponse>>;

    public sealed record FinishWorkoutSessionCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            WorkoutRating? OverallRating = null,
            string? GeneralNotes = null,
            int? CaloriesBurned = null)
        : IRequest<Result<WorkoutSessionSummaryResponse>>;

    internal static WorkoutSessionSummaryResponse Summary(WorkoutSession ws, TrainingPlan? plan)
    {
        var agg = ws.AggregateWorkoutSessionTotals();
        return new WorkoutSessionSummaryResponse(
            ws.Id,
            ws.TrainingPlanId,
            plan?.Name,
            ws.TrainingSessionId,
            ws.Name,
            ws.Status,
            ws.StartedAt,
            ws.FinishedAt,
            agg.seconds,
            agg.activeSeconds,
            ws.OverallRating,
            ws.WeekNumberInPlan,
            agg.excCount,
            agg.completedSets,
            agg.totalVolume);
    }

    public sealed class PauseWorkoutSessionCommandHandler : IRequestHandler<PauseWorkoutSessionCommand, Result<WorkoutSessionSummaryResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<PauseWorkoutSessionCommandHandler> _logger;

        public PauseWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<PauseWorkoutSessionCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<WorkoutSessionSummaryResponse>> Handle(PauseWorkoutSessionCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutSessionSummaryResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != request.CurrentUserId)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));
            if (ws.Status == WorkoutStatus.Completed || ws.Status == WorkoutStatus.Interrupted)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutSessionAlreadyFinished));
            if (ws.Status != WorkoutStatus.InProgress)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutInvalidStatusTransition));

            try { ws.Pause(); }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Status inválido ao pausar sessão {Id}.", ws.Id);
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutInvalidStatusTransition, ex.Message));
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: PauseWorkout {Id}.", ws.Id);
            return Result.Success(Summary(ws, ws.TrainingPlan));
        }
    }

    public sealed class ResumeWorkoutSessionCommandHandler : IRequestHandler<ResumeWorkoutSessionCommand, Result<WorkoutSessionSummaryResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<ResumeWorkoutSessionCommandHandler> _logger;

        public ResumeWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<ResumeWorkoutSessionCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<WorkoutSessionSummaryResponse>> Handle(ResumeWorkoutSessionCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutSessionSummaryResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != request.CurrentUserId)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));
            if (ws.Status != WorkoutStatus.Paused)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutInvalidStatusTransition));

            try { ws.Resume(); }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutInvalidStatusTransition, ex.Message));
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: ResumeWorkout {Id}.", ws.Id);
            return Result.Success(Summary(ws, ws.TrainingPlan));
        }
    }

    public sealed class FinishWorkoutSessionCommandHandler : IRequestHandler<FinishWorkoutSessionCommand, Result<WorkoutSessionSummaryResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<FinishWorkoutSessionCommandHandler> _logger;

        public FinishWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<FinishWorkoutSessionCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<WorkoutSessionSummaryResponse>> Handle(FinishWorkoutSessionCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutSessionSummaryResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != request.CurrentUserId)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutNotInProgressOrPaused));

            try { ws.Finish(request.OverallRating, request.GeneralNotes, request.CaloriesBurned); }
            catch (ArgumentException ex)
            {
                return Result.Failure<WorkoutSessionSummaryResponse>(Error.Failure(ErrorCodes.WorkoutRatingInvalidForInterrupted, ex.Message));
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: FinishWorkout {Id}.", ws.Id);
            return Result.Success(Summary(ws, ws.TrainingPlan));
        }
    }
}
