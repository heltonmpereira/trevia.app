using MediatR;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;
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
        : IQuery<WorkoutSessionsPagedResponse>;

    public sealed record GetCurrentActiveWorkoutSessionQuery(Guid CurrentUserId)
        : IQuery<WorkoutSessionDetailResponse?>;

    public sealed record GetWorkoutSessionByIdQuery(Guid CurrentUserId, Guid WorkoutSessionId)
        : IQuery<WorkoutSessionDetailResponse>;

    public sealed record GetStudentWorkoutSessionsQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            WorkoutStatus? StatusFilter = null,
            int Page = 1,
            int PageSize = 20,
            Guid? TrainingPlanId = null,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null)
        : IQuery<WorkoutSessionsPagedResponse>;

    public sealed record GetStudentCurrentActiveWorkoutSessionQuery(
        Guid CurrentUserId,
        Guid StudentId,
        bool ViewerIsAdminOrGymManager = false)
        : IQuery<WorkoutSessionDetailResponse?>;

    public sealed record GetStudentWorkoutSessionByIdQuery(
        Guid CurrentUserId,
        Guid StudentId,
        Guid WorkoutSessionId,
        bool ViewerIsAdminOrGymManager = false)
        : IQuery<WorkoutSessionDetailResponse>;

    public sealed class GetMyWorkoutSessionsQueryHandler : IRequestHandler<GetMyWorkoutSessionsQuery, WorkoutSessionsPagedResponse>
    {
        private readonly IApplicationDbContext _db;
        public GetMyWorkoutSessionsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSessionsPagedResponse> Handle(GetMyWorkoutSessionsQuery q, CancellationToken ct)
        {
            var query = BuildWorkoutSessionsListQuery(_db, q.CurrentUserId);
            query = ApplyWorkoutSessionFilters(query, q.StatusFilter, q.TrainingPlanId, q.From, q.To);
            return await BuildWorkoutSessionsPageAsync(query, q.Page, q.PageSize, ct);
        }
    }

    public sealed class GetCurrentActiveWorkoutSessionQueryHandler : IRequestHandler<GetCurrentActiveWorkoutSessionQuery, WorkoutSessionDetailResponse?>
    {
        private readonly IApplicationDbContext _db;
        public GetCurrentActiveWorkoutSessionQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSessionDetailResponse?> Handle(GetCurrentActiveWorkoutSessionQuery q, CancellationToken ct)
        {
            var ws = await BuildWorkoutSessionDetailQuery(_db)
                .FirstOrDefaultAsync(w => w.StudentId == q.CurrentUserId
                    && (w.Status == WorkoutStatus.InProgress || w.Status == WorkoutStatus.Paused), ct);

            if (ws == null) return null;
            return await BuildDetail(ws, _db, ct);
        }
    }

    public sealed class GetWorkoutSessionByIdQueryHandler : IRequestHandler<GetWorkoutSessionByIdQuery, WorkoutSessionDetailResponse>
    {
        private readonly IApplicationDbContext _db;
        public GetWorkoutSessionByIdQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSessionDetailResponse> Handle(GetWorkoutSessionByIdQuery q, CancellationToken ct)
        {
            var ws = await BuildWorkoutSessionDetailQuery(_db)
                .FirstOrDefaultAsync(w => w.Id == q.WorkoutSessionId, ct);
            if (ws == null) throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);
            if (ws.StudentId != q.CurrentUserId)
                throw new DomainException("Apenas o dono pode visualizar esta sessão.", ErrorCodes.WorkoutCannotStartNotOwner);

            return await BuildDetail(ws, _db, ct);
        }
    }

    public sealed class GetStudentWorkoutSessionsQueryHandler : IRequestHandler<GetStudentWorkoutSessionsQuery, WorkoutSessionsPagedResponse>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentWorkoutSessionsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSessionsPagedResponse> Handle(GetStudentWorkoutSessionsQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentWorkoutHistoryAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);

            var query = BuildWorkoutSessionsListQuery(_db, q.StudentId);
            query = ApplyWorkoutSessionFilters(query, q.StatusFilter, q.TrainingPlanId, q.From, q.To);
            return await BuildWorkoutSessionsPageAsync(query, q.Page, q.PageSize, ct);
        }
    }

    public sealed class GetStudentCurrentActiveWorkoutSessionQueryHandler : IRequestHandler<GetStudentCurrentActiveWorkoutSessionQuery, WorkoutSessionDetailResponse?>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentCurrentActiveWorkoutSessionQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSessionDetailResponse?> Handle(GetStudentCurrentActiveWorkoutSessionQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentWorkoutHistoryAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);

            var ws = await BuildWorkoutSessionDetailQuery(_db)
                .FirstOrDefaultAsync(w => w.StudentId == q.StudentId
                    && (w.Status == WorkoutStatus.InProgress || w.Status == WorkoutStatus.Paused), ct);

            if (ws == null) return null;
            return await BuildDetail(ws, _db, ct);
        }
    }

    public sealed class GetStudentWorkoutSessionByIdQueryHandler : IRequestHandler<GetStudentWorkoutSessionByIdQuery, WorkoutSessionDetailResponse>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentWorkoutSessionByIdQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSessionDetailResponse> Handle(GetStudentWorkoutSessionByIdQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentWorkoutHistoryAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);

            var ws = await BuildWorkoutSessionDetailQuery(_db)
                .FirstOrDefaultAsync(w => w.Id == q.WorkoutSessionId && w.StudentId == q.StudentId, ct);

            if (ws == null)
                throw new DomainException("Sessão de treino não encontrada para o aluno informado.", ErrorCodes.NotFound);

            return await BuildDetail(ws, _db, ct);
        }
    }

    private static IQueryable<WorkoutSession> BuildWorkoutSessionsListQuery(IApplicationDbContext db, Guid studentId)
        => db.Set<WorkoutSession>()
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .Include(w => w.TrainingPlan)
            .Where(w => w.StudentId == studentId)
            .AsNoTracking();

    private static IQueryable<WorkoutSession> BuildWorkoutSessionDetailQuery(IApplicationDbContext db)
        => db.Set<WorkoutSession>()
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .Include(w => w.Pauses)
            .Include(w => w.Student)
            .Include(w => w.TrainingPlan)
            .AsNoTracking();

    private static IQueryable<WorkoutSession> ApplyWorkoutSessionFilters(
        IQueryable<WorkoutSession> query,
        WorkoutStatus? statusFilter,
        Guid? trainingPlanId,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (statusFilter.HasValue) query = query.Where(w => w.Status == statusFilter.Value);
        if (trainingPlanId.HasValue) query = query.Where(w => w.TrainingPlanId == trainingPlanId.Value);
        if (from.HasValue) query = query.Where(w => w.StartedAt >= from.Value);
        if (to.HasValue) query = query.Where(w => w.StartedAt <= to.Value);

        return query.OrderByDescending(w => w.StartedAt ?? w.CreatedAt);
    }

    private static async Task<WorkoutSessionsPagedResponse> BuildWorkoutSessionsPageAsync(
        IQueryable<WorkoutSession> query,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var total = await query.CountAsync(ct);
        var skip = (safePage - 1) * safePageSize;
        var list = await query.Skip(skip).Take(safePageSize).ToListAsync(ct);

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

        return new WorkoutSessionsPagedResponse
        {
            Items = summaries,
            PageIndex = safePage,
            PageSize = safePageSize,
            TotalCount = total,
            HasNextPage = (safePage * safePageSize) < total
        };
    }

    private static async Task EnsureCanViewStudentWorkoutHistoryAsync(
        IApplicationDbContext db,
        Guid currentUserId,
        Guid studentId,
        bool viewerIsAdminOrGymManager,
        CancellationToken ct)
    {
        if (currentUserId == studentId || viewerIsAdminOrGymManager)
        {
            return;
        }

        var hasPermission = await db.Set<CoachStudentLink>()
            .AnyAsync(
                link => link.CoachId == currentUserId
                    && link.StudentId == studentId
                    && link.IsActive
                    && (((int)link.Permissions & (int)CoachPermissions.CanViewWorkoutHistory) != 0),
                ct);

        if (!hasPermission)
        {
            throw new DomainException(
                "Você não tem permissão para visualizar o histórico deste aluno.",
                ErrorCodes.Forbidden);
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
