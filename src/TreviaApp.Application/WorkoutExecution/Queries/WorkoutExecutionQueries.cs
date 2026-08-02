using MediatR;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Enums;
using TreviaApp.Shared.Errors;
using TreviaApp.Application.WorkoutExecution.Mappings;
using static TreviaApp.Application.WorkoutExecution.Commands.ExerciseActions;

namespace TreviaApp.Application.WorkoutExecution.Queries;

public static class Queries
{
    public sealed record GetMyWorkoutSessionsQuery(
            Guid CurrentUserId,
            WorkoutStatus? StatusFilter = null,
            int Page = 1,
            int PageSize = 20,
            Guid? TrainingPlanId = null,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null)
        : IRequest<Result<WorkoutSessionsPagedResponse>>;

    public sealed record GetCurrentActiveWorkoutSessionQuery(Guid CurrentUserId)
        : IRequest<Result<WorkoutSessionDetailResponse?>>;

    public sealed record GetWorkoutSessionByIdQuery(Guid CurrentUserId, Guid WorkoutSessionId)
        : IRequest<Result<WorkoutSessionDetailResponse>>;

    public sealed class GetMyWorkoutSessionsQueryHandler : IRequestHandler<GetMyWorkoutSessionsQuery, Result<WorkoutSessionsPagedResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetMyWorkoutSessionsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<Result<WorkoutSessionsPagedResponse>> Handle(GetMyWorkoutSessionsQuery q, CancellationToken ct)
        {
            var query = _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.TrainingPlan)
                .Where(w => w.StudentId == q.CurrentUserId)
                .AsNoTracking();

            if (q.StatusFilter.HasValue) query = query.Where(w => w.Status == q.StatusFilter.Value);
            if (q.TrainingPlanId.HasValue) query = query.Where(w => w.TrainingPlanId == q.TrainingPlanId.Value);
            if (q.From.HasValue) query = query.Where(w => w.StartedAt >= q.From.Value);
            if (q.To.HasValue) query = query.Where(w => w.StartedAt <= q.To.Value);

            query = query.OrderByDescending(w => w.StartedAt ?? w.CreatedAt);

            var total = await query.CountAsync(ct);
            var skip = Math.Max(0, q.Page - 1) * q.PageSize;
            var list = await query.Skip(skip).Take(q.PageSize).ToListAsync(ct);

            var summaries = list.Select(ws =>
            {
                var agg = ws.AggregateWorkoutSessionTotals();
                return new WorkoutSessionSummaryResponse(
                    ws.Id,
                    ws.TrainingPlanId,
                    ws.TrainingPlan?.Name,
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
            }).ToList();

            var response = new WorkoutSessionsPagedResponse
            {
                Items = summaries,
                Page = q.Page,
                PageSize = q.PageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)Math.Max(1, q.PageSize))
            };
            return Result.Success(response);
        }
    }

    public sealed class GetCurrentActiveWorkoutSessionQueryHandler : IRequestHandler<GetCurrentActiveWorkoutSessionQuery, Result<WorkoutSessionDetailResponse?>>
    {
        private readonly IApplicationDbContext _db;
        public GetCurrentActiveWorkoutSessionQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<Result<WorkoutSessionDetailResponse?>> Handle(GetCurrentActiveWorkoutSessionQuery q, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.Student)
                .Include(w => w.TrainingPlan)
                .FirstOrDefaultAsync(w => w.StudentId == q.CurrentUserId
                    && (w.Status == WorkoutStatus.InProgress || w.Status == WorkoutStatus.Paused), ct);

            if (ws == null) return Result.Success<WorkoutSessionDetailResponse?>(null);
            return Result.Success<WorkoutSessionDetailResponse?>(await BuildDetail(ws, _db, ct));
        }
    }

    public sealed class GetWorkoutSessionByIdQueryHandler : IRequestHandler<GetWorkoutSessionByIdQuery, Result<WorkoutSessionDetailResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetWorkoutSessionByIdQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<Result<WorkoutSessionDetailResponse>> Handle(GetWorkoutSessionByIdQuery q, CancellationToken ct)
        {
            var ws = await _db.Set<WorkoutSession>()
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Include(w => w.Pauses)
                .Include(w => w.Student)
                .Include(w => w.TrainingPlan)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == q.WorkoutSessionId, ct);
            if (ws == null) return Result.Failure<WorkoutSessionDetailResponse>(Error.NotFound(ErrorCodes.WorkoutSessionNotFound));
            if (ws.StudentId != q.CurrentUserId)
                return Result.Failure<WorkoutSessionDetailResponse>(Error.Failure(ErrorCodes.WorkoutCannotStartNotOwner));

            return Result.Success(await BuildDetail(ws, _db, ct));
        }
    }

    internal static async Task<WorkoutSessionDetailResponse> BuildDetail(WorkoutSession ws, IApplicationDbContext db, CancellationToken ct)
    {
        var exerciseIds = ws.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var exerciseNames = await db.Set<TreviaApp.Domain.Exercises.Exercise>()
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(k => k.Id, v => v.Name, ct);

        var userPhotoFileId = await db.Set<TreviaApp.Domain.Profiles.UserProfile>()
            .Where(up => up.UserId == ws.StudentId)
            .Select(up => up.Photo != null ? up.Photo.FileId : null)
            .FirstOrDefaultAsync(ct);

        var pauses = ws.Pauses
            .OrderBy(p => p.StartedAt)
            .Select(p => new WorkoutPauseResponse(
                p.Id,
                p.StartedAt,
                p.EndedAt,
                p.Duration.HasValue ? (long?)p.Duration.Value.TotalSeconds : null))
            .ToList();

        var agg = ws.AggregateWorkoutSessionTotals();

        return new WorkoutSessionDetailResponse(
            ws.Id,
            ws.StudentId,
            ws.Student?.DisplayName ?? "Aluno",
            userPhotoFileId,
            ws.TrainingPlanId,
            ws.TrainingPlan?.Name,
            ws.TrainingSessionId,
            ws.Name,
            ws.Status,
            ws.StartedAt,
            ws.FinishedAt,
            agg.seconds,
            agg.activeSeconds,
            ws.CaloriesBurned,
            ws.OverallRating,
            ws.GeneralNotes,
            ws.WeekNumberInPlan,
            ws.Exercises.OrderBy(e => e.Order).Select(wex =>
            {
                exerciseNames.TryGetValue(wex.ExerciseId, out var exName);
                return MapExercise(wex, exName);
            }).ToList(),
            pauses);
    }
}
