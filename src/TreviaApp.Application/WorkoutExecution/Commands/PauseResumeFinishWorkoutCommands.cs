using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.WorkoutExecution.Mappings;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Application.WorkoutExecution.Commands;

public static class PauseResumeFinish
{
    public sealed record PauseWorkoutSessionCommand(Guid CurrentUserId, Guid WorkoutSessionId)
        : ICommand<WorkoutSessionSummaryResponse>;

    public sealed record ResumeWorkoutSessionCommand(Guid CurrentUserId, Guid WorkoutSessionId)
        : ICommand<WorkoutSessionSummaryResponse>;

    public sealed record FinishWorkoutSessionCommand(
            Guid CurrentUserId,
            Guid WorkoutSessionId,
            WorkoutRating? OverallRating = null,
            string? GeneralNotes = null,
            int? CaloriesBurned = null)
        : ICommand<WorkoutSessionSummaryResponse>;

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

    public sealed class PauseWorkoutSessionCommandHandler : IRequestHandler<PauseWorkoutSessionCommand, WorkoutSessionSummaryResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<PauseWorkoutSessionCommandHandler> _logger;

        public PauseWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<PauseWorkoutSessionCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkoutSessionSummaryResponse> Handle(PauseWorkoutSessionCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != request.CurrentUserId)
                throw new DomainException("Apenas o dono da sessão pode pausar o treino.", ErrorCodes.WorkoutCannotStartNotOwner);
            if (ws.Status == WorkoutStatus.Completed || ws.Status == WorkoutStatus.Interrupted)
                throw new DomainException("Sessão já foi finalizada.", ErrorCodes.WorkoutSessionAlreadyFinished);
            if (ws.Status != WorkoutStatus.InProgress)
                throw new DomainException("Apenas sessões em andamento podem ser pausadas.", ErrorCodes.WorkoutInvalidStatusTransition);

            try { ws.Pause(); }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Status inválido ao pausar sessão {Id}.", ws.Id);
                throw new DomainException("Não foi possível pausar o treino.", ErrorCodes.WorkoutInvalidStatusTransition, ex.Message);
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: PauseWorkout {Id}.", ws.Id);
            return Summary(ws, ws.TrainingPlan);
        }
    }

    public sealed class ResumeWorkoutSessionCommandHandler : IRequestHandler<ResumeWorkoutSessionCommand, WorkoutSessionSummaryResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<ResumeWorkoutSessionCommandHandler> _logger;

        public ResumeWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<ResumeWorkoutSessionCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkoutSessionSummaryResponse> Handle(ResumeWorkoutSessionCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != request.CurrentUserId)
                throw new DomainException("Apenas o dono da sessão pode retomar o treino.", ErrorCodes.WorkoutCannotStartNotOwner);
            if (ws.Status != WorkoutStatus.Paused)
                throw new DomainException("Apenas sessões pausadas podem ser retomadas.", ErrorCodes.WorkoutInvalidStatusTransition);

            try { ws.Resume(); }
            catch (InvalidOperationException ex)
            {
                throw new DomainException("Não foi possível retomar o treino.", ErrorCodes.WorkoutInvalidStatusTransition, ex.Message);
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: ResumeWorkout {Id}.", ws.Id);
            return Summary(ws, ws.TrainingPlan);
        }
    }

    public sealed class FinishWorkoutSessionCommandHandler : IRequestHandler<FinishWorkoutSessionCommand, WorkoutSessionSummaryResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<FinishWorkoutSessionCommandHandler> _logger;

        public FinishWorkoutSessionCommandHandler(IApplicationDbContext db, ILogger<FinishWorkoutSessionCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkoutSessionSummaryResponse> Handle(FinishWorkoutSessionCommand request, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.Id == request.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != request.CurrentUserId)
                throw new DomainException("Apenas o dono da sessão pode finalizar o treino.", ErrorCodes.WorkoutCannotStartNotOwner);
            if (ws.Status != WorkoutStatus.InProgress && ws.Status != WorkoutStatus.Paused)
                throw new DomainException("Apenas sessões em andamento ou pausadas podem ser finalizadas.", ErrorCodes.WorkoutNotInProgressOrPaused);

            try { ws.Finish(request.OverallRating, request.GeneralNotes, request.CaloriesBurned); }
            catch (ArgumentException ex)
            {
                throw new DomainException("Dados de avaliação inválidos.", ErrorCodes.WorkoutRatingInvalidForInterrupted, ex.Message);
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SaveChangesAsync explícito concluído: FinishWorkout {Id}.", ws.Id);
            return Summary(ws, ws.TrainingPlan);
        }
    }
}
