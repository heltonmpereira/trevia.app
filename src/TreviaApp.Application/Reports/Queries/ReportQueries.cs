using MediatR;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Reports.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Exercises;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Application.Reports.Queries;

public static class ReportQueries
{
    public sealed record GetMyWorkoutSummaryQuery(
            Guid CurrentUserId,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null,
            Guid? TrainingPlanId = null)
        : IQuery<WorkoutSummaryResponse>;

    public sealed record GetMyWorkoutCalendarQuery(
            Guid CurrentUserId,
            int? Year = null,
            int? Month = null)
        : IQuery<IReadOnlyList<WorkoutCalendarDayResponse>>;

    public sealed record GetMyProgressOverTimeQuery(
            Guid CurrentUserId,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null,
            ProgressGranularity Granularity = ProgressGranularity.Week)
        : IQuery<IReadOnlyList<WorkoutProgressPointResponse>>;

    public sealed record GetMyMuscleVolumeDistributionQuery(
            Guid CurrentUserId,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null)
        : IQuery<IReadOnlyList<MuscleVolumeItemResponse>>;

    public sealed record GetMyMostPerformedExercisesQuery(
            Guid CurrentUserId,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null,
            int Top = 10,
            ExerciseRankBy RankBy = ExerciseRankBy.Volume)
        : IQuery<IReadOnlyList<ExerciseRankItemResponse>>;

    public sealed record GetMyPersonalRecordsQuery(
            Guid CurrentUserId,
            Guid? ExerciseId = null)
        : IQuery<IReadOnlyList<PersonalRecordItemResponse>>;

    public sealed record GetStudentWorkoutSummaryQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null,
            Guid? TrainingPlanId = null)
        : IQuery<WorkoutSummaryResponse>;

    public sealed record GetStudentWorkoutCalendarQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            int? Year = null,
            int? Month = null)
        : IQuery<IReadOnlyList<WorkoutCalendarDayResponse>>;

    public sealed record GetStudentProgressOverTimeQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null,
            ProgressGranularity Granularity = ProgressGranularity.Week)
        : IQuery<IReadOnlyList<WorkoutProgressPointResponse>>;

    public sealed record GetStudentMuscleVolumeDistributionQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null)
        : IQuery<IReadOnlyList<MuscleVolumeItemResponse>>;

    public sealed record GetStudentMostPerformedExercisesQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            DateTimeOffset? From = null,
            DateTimeOffset? To = null,
            int Top = 10,
            ExerciseRankBy RankBy = ExerciseRankBy.Volume)
        : IQuery<IReadOnlyList<ExerciseRankItemResponse>>;

    public sealed record GetStudentPersonalRecordsQuery(
            Guid CurrentUserId,
            Guid StudentId,
            bool ViewerIsAdminOrGymManager = false,
            Guid? ExerciseId = null)
        : IQuery<IReadOnlyList<PersonalRecordItemResponse>>;

    private static DateTimeOffset SafeFrom(DateTimeOffset? from)
        => from ?? DateTimeOffset.UtcNow.AddDays(-30);

    private static DateTimeOffset SafeTo(DateTimeOffset? to)
        => to ?? DateTimeOffset.UtcNow;

    private static IQueryable<WorkoutSession> FilterSessionsForStudent(
        IApplicationDbContext db,
        Guid studentId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? trainingPlanId)
    {
        var q = db.Set<WorkoutSession>()
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .Where(w => w.StudentId == studentId
                        && w.StartedAt >= from
                        && w.StartedAt <= to
                        && (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.Interrupted));
        if (trainingPlanId.HasValue)
            q = q.Where(w => w.TrainingPlanId == trainingPlanId.Value);
        return q.AsNoTracking();
    }

    private static async Task EnsureCanViewStudentReportsAsync(
        IApplicationDbContext db,
        Guid currentUserId,
        Guid studentId,
        bool viewerIsAdminOrGymManager,
        CancellationToken ct)
    {
        if (currentUserId == studentId || viewerIsAdminOrGymManager) return;

        var allowed = await db.Set<CoachStudentLink>()
            .AnyAsync(l =>
                l.CoachId == currentUserId
                && l.StudentId == studentId
                && l.IsActive
                && (((int)l.Permissions & (int)CoachPermissions.CanViewWorkoutHistory) != 0), ct);

        if (!allowed)
            throw new DomainException("Você não tem permissão para visualizar relatórios deste aluno.", ErrorCodes.Forbidden);
    }

    public sealed class GetMyWorkoutSummaryQueryHandler : IRequestHandler<GetMyWorkoutSummaryQuery, WorkoutSummaryResponse>
    {
        private readonly IApplicationDbContext _db;
        public GetMyWorkoutSummaryQueryHandler(IApplicationDbContext db) { _db = db; }

        public Task<WorkoutSummaryResponse> Handle(GetMyWorkoutSummaryQuery q, CancellationToken ct)
            => BuildSummaryAsync(_db, q.CurrentUserId, SafeFrom(q.From), SafeTo(q.To), q.TrainingPlanId, ct);
    }

    public sealed class GetStudentWorkoutSummaryQueryHandler : IRequestHandler<GetStudentWorkoutSummaryQuery, WorkoutSummaryResponse>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentWorkoutSummaryQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<WorkoutSummaryResponse> Handle(GetStudentWorkoutSummaryQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentReportsAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);
            return await BuildSummaryAsync(_db, q.StudentId, SafeFrom(q.From), SafeTo(q.To), q.TrainingPlanId, ct);
        }
    }

    private static async Task<WorkoutSummaryResponse> BuildSummaryAsync(
        IApplicationDbContext db,
        Guid studentId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? trainingPlanId,
        CancellationToken ct)
    {
        var sessions = await FilterSessionsForStudent(db, studentId, from, to, trainingPlanId).ToListAsync(ct);

        var allSets = sessions.SelectMany(s => s.Exercises).SelectMany(e => e.Sets).ToList();
        var completedSets = allSets.Where(s => s.Completed).ToList();
        var volumes = completedSets.Where(s => s.VolumeKg.HasValue).Select(s => s.VolumeKg!.Value).ToList();
        var distances = completedSets.Where(s => s.DistanceKm.HasValue).Select(s => s.DistanceKm!.Value).ToList();
        var calories = completedSets.Where(s => s.Calories.HasValue).Select(s => s.Calories!.Value).ToList();

        var totalWorkouts = sessions.Count;
        var completedWorkouts = sessions.Count(s => s.Status == WorkoutStatus.Completed);
        var totalSets = allSets.Count;
        var completedSetsCount = completedSets.Count;
        var completionRatePercent = totalSets == 0 ? 0 : Math.Round((decimal)completedSetsCount / totalSets * 100, 2);
        var totalVolumeKg = volumes.Any() ? (decimal?)volumes.Sum() : null;
        var totalDurationSeconds = sessions
            .Where(s => s.TotalDurationElapsed.HasValue)
            .Sum(s => (long?)s.TotalDurationElapsed!.Value.TotalSeconds) ?? 0L;
        var totalActiveSeconds = sessions
            .Where(s => s.ActiveTime.HasValue)
            .Sum(s => (long?)s.ActiveTime!.Value.TotalSeconds) ?? 0L;
        var avgDuration = totalWorkouts == 0 ? null : (long?)(totalDurationSeconds / totalWorkouts);
        var avgActive = totalWorkouts == 0 ? null : (long?)(totalActiveSeconds / totalWorkouts);
        var avgVolume = totalWorkouts == 0 || !totalVolumeKg.HasValue ? null : (decimal?)Math.Round(totalVolumeKg.Value / totalWorkouts, 2);
        var uniqueExercises = sessions.SelectMany(s => s.Exercises).Select(e => e.ExerciseId).Distinct().Count();
        var totalDistance = distances.Any() ? (decimal?)Math.Round(distances.Sum(), 3) : null;
        var totalCaloriesSum = calories.Any() ? (int?)calories.Sum() : null;

        var (currentStreak, longestStreak) = CalculateStreaks(sessions);

        return new WorkoutSummaryResponse(
            from, to,
            totalWorkouts, completedWorkouts,
            totalSets, completedSetsCount, completionRatePercent,
            totalVolumeKg, totalDurationSeconds, totalActiveSeconds,
            avgDuration, avgActive, avgVolume,
            uniqueExercises, totalDistance, totalCaloriesSum,
            currentStreak, longestStreak);
    }

    private static (int current, int longest) CalculateStreaks(List<WorkoutSession> sessions)
    {
        var days = sessions
            .Where(s => s.StartedAt.HasValue)
            .Select(s => DateOnly.FromDateTime(s.StartedAt!.Value.Date))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        if (days.Count == 0) return (0, 0);

        var longest = 0;
        var currentRun = 1;
        for (var i = 1; i < days.Count; i++)
        {
            if (days[i].DayNumber - days[i - 1].DayNumber == 1)
            {
                currentRun++;
                longest = Math.Max(longest, currentRun);
            }
            else
            {
                longest = Math.Max(longest, currentRun);
                currentRun = 1;
            }
        }
        longest = Math.Max(longest, currentRun);

        var current = 0;
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var check = today;
        while (days.Contains(check))
        {
            current++;
            check = check.AddDays(-1);
        }
        if (current == 0 && days.Contains(today.AddDays(-1)))
        {
            check = today.AddDays(-1);
            while (days.Contains(check))
            {
                current++;
                check = check.AddDays(-1);
            }
        }
        return (current, longest);
    }

    public sealed class GetMyWorkoutCalendarQueryHandler : IRequestHandler<GetMyWorkoutCalendarQuery, IReadOnlyList<WorkoutCalendarDayResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetMyWorkoutCalendarQueryHandler(IApplicationDbContext db) { _db = db; }

        public Task<IReadOnlyList<WorkoutCalendarDayResponse>> Handle(GetMyWorkoutCalendarQuery q, CancellationToken ct)
            => BuildCalendarAsync(_db, q.CurrentUserId, q.Year, q.Month, ct);
    }

    public sealed class GetStudentWorkoutCalendarQueryHandler : IRequestHandler<GetStudentWorkoutCalendarQuery, IReadOnlyList<WorkoutCalendarDayResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentWorkoutCalendarQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<IReadOnlyList<WorkoutCalendarDayResponse>> Handle(GetStudentWorkoutCalendarQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentReportsAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);
            return await BuildCalendarAsync(_db, q.StudentId, q.Year, q.Month, ct);
        }
    }

    private static async Task<IReadOnlyList<WorkoutCalendarDayResponse>> BuildCalendarAsync(
        IApplicationDbContext db,
        Guid studentId,
        int? year,
        int? month,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;
        var from = new DateTimeOffset(targetYear, targetMonth, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddMonths(1).AddTicks(-1);

        var sessions = await db.Set<WorkoutSession>()
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .Where(w => w.StudentId == studentId
                        && w.StartedAt >= from
                        && w.StartedAt <= to
                        && (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.Interrupted))
            .AsNoTracking()
            .ToListAsync(ct);

        return sessions
            .Where(s => s.StartedAt.HasValue)
            .GroupBy(s => DateOnly.FromDateTime(s.StartedAt!.Value.Date))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var setsCompleted = g.SelectMany(s => s.Exercises).SelectMany(e => e.Sets).Where(ss => ss.Completed).ToList();
                var vol = setsCompleted.Where(ss => ss.VolumeKg.HasValue).Select(ss => ss.VolumeKg!.Value);
                var active = g.Where(s => s.ActiveTime.HasValue).Sum(s => (long?)s.ActiveTime!.Value.TotalSeconds);
                return new WorkoutCalendarDayResponse(
                    g.Key,
                    g.Count(),
                    vol.Any() ? (decimal?)Math.Round(vol.Sum(), 2) : null,
                    active.HasValue ? active.Value : null);
            })
            .ToList();
    }

    public sealed class GetMyProgressOverTimeQueryHandler : IRequestHandler<GetMyProgressOverTimeQuery, IReadOnlyList<WorkoutProgressPointResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetMyProgressOverTimeQueryHandler(IApplicationDbContext db) { _db = db; }

        public Task<IReadOnlyList<WorkoutProgressPointResponse>> Handle(GetMyProgressOverTimeQuery q, CancellationToken ct)
            => BuildProgressAsync(_db, q.CurrentUserId, SafeFrom(q.From), SafeTo(q.To), q.Granularity, ct);
    }

    public sealed class GetStudentProgressOverTimeQueryHandler : IRequestHandler<GetStudentProgressOverTimeQuery, IReadOnlyList<WorkoutProgressPointResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentProgressOverTimeQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<IReadOnlyList<WorkoutProgressPointResponse>> Handle(GetStudentProgressOverTimeQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentReportsAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);
            return await BuildProgressAsync(_db, q.StudentId, SafeFrom(q.From), SafeTo(q.To), q.Granularity, ct);
        }
    }

    private static DateTimeOffset PeriodStartKey(DateTimeOffset d, ProgressGranularity g)
    {
        switch (g)
        {
            case ProgressGranularity.Day:
                return new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, TimeSpan.Zero);
            case ProgressGranularity.Week:
                var day = (int)d.DayOfWeek;
                var diff = (7 + (day - 0)) % 7;
                var weekStart = d.Date.AddDays(-1 * diff);
                return new DateTimeOffset(weekStart, TimeSpan.Zero);
            case ProgressGranularity.Month:
                return new DateTimeOffset(d.Year, d.Month, 1, 0, 0, 0, TimeSpan.Zero);
            default:
                goto case ProgressGranularity.Week;
        }
    }

    private static DateTimeOffset PeriodEndFromStart(DateTimeOffset start, ProgressGranularity g)
        => g switch
        {
            ProgressGranularity.Day => start.AddDays(1).AddTicks(-1),
            ProgressGranularity.Week => start.AddDays(7).AddTicks(-1),
            ProgressGranularity.Month => start.AddMonths(1).AddTicks(-1),
            _ => start.AddDays(7).AddTicks(-1)
        };

    private static async Task<IReadOnlyList<WorkoutProgressPointResponse>> BuildProgressAsync(
        IApplicationDbContext db,
        Guid studentId,
        DateTimeOffset from,
        DateTimeOffset to,
        ProgressGranularity granularity,
        CancellationToken ct)
    {
        var sessions = await db.Set<WorkoutSession>()
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .Where(w => w.StudentId == studentId
                        && w.StartedAt >= from
                        && w.StartedAt <= to
                        && (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.Interrupted))
            .AsNoTracking()
            .ToListAsync(ct);

        var groups = sessions
            .Where(s => s.StartedAt.HasValue)
            .GroupBy(s => PeriodStartKey(s.StartedAt!.Value, granularity))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var start = g.Key;
                var end = PeriodEndFromStart(start, granularity);
                var wkts = g.ToList();
                var setsCompleted = wkts.SelectMany(s => s.Exercises).SelectMany(e => e.Sets).Where(ss => ss.Completed).ToList();
                var vol = setsCompleted.Where(ss => ss.VolumeKg.HasValue).Select(ss => ss.VolumeKg!.Value);
                var dur = wkts.Where(s => s.TotalDurationElapsed.HasValue).Sum(s => (long)s.TotalDurationElapsed!.Value.TotalSeconds);
                var act = wkts.Where(s => s.ActiveTime.HasValue).Sum(s => (long)s.ActiveTime!.Value.TotalSeconds);
                var dist = setsCompleted.Where(ss => ss.DistanceKm.HasValue).Select(ss => ss.DistanceKm!.Value);
                var cal = setsCompleted.Where(ss => ss.Calories.HasValue).Select(ss => ss.Calories!.Value);
                return new WorkoutProgressPointResponse(
                    start, end,
                    wkts.Count,
                    setsCompleted.Count,
                    vol.Any() ? (decimal?)Math.Round(vol.Sum(), 2) : null,
                    dur, act,
                    dist.Any() ? (decimal?)Math.Round(dist.Sum(), 3) : null,
                    cal.Any() ? (int?)cal.Sum() : null);
            })
            .ToList();
        return groups;
    }

    public sealed class GetMyMuscleVolumeDistributionQueryHandler : IRequestHandler<GetMyMuscleVolumeDistributionQuery, IReadOnlyList<MuscleVolumeItemResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetMyMuscleVolumeDistributionQueryHandler(IApplicationDbContext db) { _db = db; }

        public Task<IReadOnlyList<MuscleVolumeItemResponse>> Handle(GetMyMuscleVolumeDistributionQuery q, CancellationToken ct)
            => BuildMuscleDistributionAsync(_db, q.CurrentUserId, SafeFrom(q.From), SafeTo(q.To), ct);
    }

    public sealed class GetStudentMuscleVolumeDistributionQueryHandler : IRequestHandler<GetStudentMuscleVolumeDistributionQuery, IReadOnlyList<MuscleVolumeItemResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentMuscleVolumeDistributionQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<IReadOnlyList<MuscleVolumeItemResponse>> Handle(GetStudentMuscleVolumeDistributionQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentReportsAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);
            return await BuildMuscleDistributionAsync(_db, q.StudentId, SafeFrom(q.From), SafeTo(q.To), ct);
        }
    }

    private static async Task<IReadOnlyList<MuscleVolumeItemResponse>> BuildMuscleDistributionAsync(
        IApplicationDbContext db,
        Guid studentId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        var validSetIds = await db.Set<WorkoutSession>()
            .Where(w => w.StudentId == studentId
                        && w.StartedAt >= from
                        && w.StartedAt <= to
                        && (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.Interrupted))
            .SelectMany(w => w.Exercises)
            .SelectMany(e => e.Sets)
            .Where(s => s.Completed && s.VolumeKg.HasValue)
            .Select(s => new { s.Id, s.WorkoutExercise.ExerciseId, Volume = s.VolumeKg!.Value })
            .AsNoTracking()
            .ToListAsync(ct);

        if (validSetIds.Count == 0)
            return Array.Empty<MuscleVolumeItemResponse>();

        var exIds = validSetIds.Select(x => x.ExerciseId).Distinct().ToList();
        var muscleLinks = await db.Set<ExerciseMuscle>()
            .Where(em => exIds.Contains(em.ExerciseId))
            .AsNoTracking()
            .ToListAsync(ct);

        var volByExId = validSetIds.GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Volume));

        var setsByExId = validSetIds.GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var muscleTotals = new Dictionary<(Muscle m, MuscleRole? r), (decimal vol, int sets)>();
        decimal totalAll = 0;

        foreach (var em in muscleLinks)
        {
            if (!volByExId.TryGetValue(em.ExerciseId, out var v)) continue;
            var setsCount = setsByExId.TryGetValue(em.ExerciseId, out var sc) ? sc : 0;
            var activationFactor = em.ActivationPercent.HasValue ? (em.ActivationPercent.Value / 100m) : (em.MuscleRole == MuscleRole.Primary ? 1.0m : 0.5m);
            var allocated = Math.Round(v * activationFactor, 2);
            var k = (em.Muscle, em.MuscleRole);
            if (!muscleTotals.ContainsKey(k)) muscleTotals[k] = (0, 0);
            muscleTotals[k] = (muscleTotals[k].vol + allocated, muscleTotals[k].sets + setsCount);
            totalAll += allocated;
        }

        return muscleTotals
            .OrderByDescending(t => t.Value.vol)
            .Select(t => new MuscleVolumeItemResponse(
                t.Key.m,
                t.Key.r,
                Math.Round(t.Value.vol, 2),
                t.Value.sets,
                totalAll == 0 ? 0 : Math.Round(t.Value.vol / totalAll * 100, 2)))
            .ToList();
    }

    public sealed class GetMyMostPerformedExercisesQueryHandler : IRequestHandler<GetMyMostPerformedExercisesQuery, IReadOnlyList<ExerciseRankItemResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetMyMostPerformedExercisesQueryHandler(IApplicationDbContext db) { _db = db; }

        public Task<IReadOnlyList<ExerciseRankItemResponse>> Handle(GetMyMostPerformedExercisesQuery q, CancellationToken ct)
            => BuildTopExercisesAsync(_db, q.CurrentUserId, SafeFrom(q.From), SafeTo(q.To), Math.Clamp(q.Top, 1, 100), q.RankBy, ct);
    }

    public sealed class GetStudentMostPerformedExercisesQueryHandler : IRequestHandler<GetStudentMostPerformedExercisesQuery, IReadOnlyList<ExerciseRankItemResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentMostPerformedExercisesQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<IReadOnlyList<ExerciseRankItemResponse>> Handle(GetStudentMostPerformedExercisesQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentReportsAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);
            return await BuildTopExercisesAsync(_db, q.StudentId, SafeFrom(q.From), SafeTo(q.To), Math.Clamp(q.Top, 1, 100), q.RankBy, ct);
        }
    }

    private static async Task<IReadOnlyList<ExerciseRankItemResponse>> BuildTopExercisesAsync(
        IApplicationDbContext db,
        Guid studentId,
        DateTimeOffset from,
        DateTimeOffset to,
        int top,
        ExerciseRankBy rankBy,
        CancellationToken ct)
    {
        var sessionsQuery = db.Set<WorkoutSession>()
            .Where(w => w.StudentId == studentId
                        && w.StartedAt >= from
                        && w.StartedAt <= to
                        && (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.Interrupted));

        var flat = await sessionsQuery
            .SelectMany(w => w.Exercises, (w, e) => new
            {
                WorkoutId = w.Id,
                e.ExerciseId,
                Sets = e.Sets
            })
            .AsNoTracking()
            .ToListAsync(ct);

        var exIds = flat.Select(x => x.ExerciseId).Distinct().ToList();
        var exNames = await db.Set<Exercise>()
            .Where(e => exIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, ct);

        var grouped = flat
            .GroupBy(x => x.ExerciseId)
            .Select(g =>
            {
                var allSets = g.SelectMany(x => x.Sets).ToList();
                var completed = allSets.Where(s => s.Completed).ToList();
                var vol = completed.Where(s => s.VolumeKg.HasValue).Select(s => s.VolumeKg!.Value);
                var dur = completed.Where(s => s.ActualDuration.HasValue).Sum(s => (long?)s.ActualDuration!.Value.TotalSeconds);
                var dist = completed.Where(s => s.DistanceKm.HasValue).Select(s => s.DistanceKm!.Value);
                exNames.TryGetValue(g.Key, out var nm);
                return new
                {
                    ExerciseId = g.Key,
                    ExerciseName = nm ?? "Exercício",
                    WorkoutsCount = g.Select(x => x.WorkoutId).Distinct().Count(),
                    SetsCount = allSets.Count,
                    CompletedSetsCount = completed.Count,
                    Volume = vol.Any() ? (decimal?)Math.Round(vol.Sum(), 2) : null,
                    Duration = dur,
                    Distance = dist.Any() ? (decimal?)Math.Round(dist.Sum(), 3) : null
                };
            })
            .ToList();

        var ordered = rankBy switch
        {
            ExerciseRankBy.Frequency => grouped.OrderByDescending(x => x.WorkoutsCount).ThenByDescending(x => x.Volume ?? 0),
            ExerciseRankBy.Sets => grouped.OrderByDescending(x => x.CompletedSetsCount).ThenByDescending(x => x.Volume ?? 0),
            _ => grouped.OrderByDescending(x => x.Volume ?? 0).ThenByDescending(x => x.WorkoutsCount)
        };

        return ordered
            .Take(top)
            .Select((x, i) => new ExerciseRankItemResponse(
                x.ExerciseId,
                x.ExerciseName,
                i + 1,
                x.WorkoutsCount,
                x.SetsCount,
                x.CompletedSetsCount,
                x.Volume,
                x.Duration,
                x.Distance))
            .ToList();
    }

    public sealed class GetMyPersonalRecordsQueryHandler : IRequestHandler<GetMyPersonalRecordsQuery, IReadOnlyList<PersonalRecordItemResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetMyPersonalRecordsQueryHandler(IApplicationDbContext db) { _db = db; }

        public Task<IReadOnlyList<PersonalRecordItemResponse>> Handle(GetMyPersonalRecordsQuery q, CancellationToken ct)
            => BuildPersonalRecordsAsync(_db, q.CurrentUserId, q.ExerciseId, ct);
    }

    public sealed class GetStudentPersonalRecordsQueryHandler : IRequestHandler<GetStudentPersonalRecordsQuery, IReadOnlyList<PersonalRecordItemResponse>>
    {
        private readonly IApplicationDbContext _db;
        public GetStudentPersonalRecordsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<IReadOnlyList<PersonalRecordItemResponse>> Handle(GetStudentPersonalRecordsQuery q, CancellationToken ct)
        {
            await EnsureCanViewStudentReportsAsync(_db, q.CurrentUserId, q.StudentId, q.ViewerIsAdminOrGymManager, ct);
            return await BuildPersonalRecordsAsync(_db, q.StudentId, q.ExerciseId, ct);
        }
    }

    private static async Task<IReadOnlyList<PersonalRecordItemResponse>> BuildPersonalRecordsAsync(
        IApplicationDbContext db,
        Guid studentId,
        Guid? exerciseId,
        CancellationToken ct)
    {
        var setsQuery = db.Set<WorkoutSession>()
            .Where(w => w.StudentId == studentId
                        && (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.Interrupted))
            .SelectMany(w => w.Exercises)
            .SelectMany(e => e.Sets, (e, s) => new
            {
                WorkoutExercise = e,
                Set = s,
                e.WorkoutSession.StartedAt,
                e.WorkoutSession.Id
            })
            .Where(x => x.Set.Completed);

        if (exerciseId.HasValue)
            setsQuery = setsQuery.Where(x => x.WorkoutExercise.ExerciseId == exerciseId.Value);

        var flatSets = await setsQuery
            .AsNoTracking()
            .ToListAsync(ct);

        if (flatSets.Count == 0)
            return Array.Empty<PersonalRecordItemResponse>();

        var exIds = flatSets.Select(x => x.WorkoutExercise.ExerciseId).Distinct().ToList();
        var exNames = await db.Set<Exercise>()
            .Where(e => exIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, ct);

        var results = new List<PersonalRecordItemResponse>();

        foreach (var exGroup in flatSets.GroupBy(x => x.WorkoutExercise.ExerciseId))
        {
            exNames.TryGetValue(exGroup.Key, out var nm);
            var exName = nm ?? "Exercício";
            var sets = exGroup.ToList();

            var maxLoad = sets
                .Where(s => s.Set.ActualLoadValue.HasValue)
                .OrderByDescending(s => s.Set.ActualLoadValue!.Value)
                .ThenByDescending(s => s.Set.ActualReps ?? 0)
                .FirstOrDefault();
            if (maxLoad != null)
                results.Add(new PersonalRecordItemResponse(
                    exGroup.Key, exName, PersonalRecordType.MaxLoad,
                    Math.Round(maxLoad.Set.ActualLoadValue!.Value, 2),
                    maxLoad.Set.ActualLoadUnit.ToString(),
                    maxLoad.Set.ActualReps,
                    maxLoad.StartedAt ?? DateTimeOffset.UtcNow,
                    maxLoad.Id,
                    maxLoad.Set.Id));

            var maxVolume = sets
                .Where(s => s.Set.VolumeKg.HasValue)
                .OrderByDescending(s => s.Set.VolumeKg!.Value)
                .FirstOrDefault();
            if (maxVolume != null)
                results.Add(new PersonalRecordItemResponse(
                    exGroup.Key, exName, PersonalRecordType.MaxVolume,
                    Math.Round(maxVolume.Set.VolumeKg!.Value, 2),
                    "kg·rep",
                    maxVolume.Set.ActualReps,
                    maxVolume.StartedAt ?? DateTimeOffset.UtcNow,
                    maxVolume.Id,
                    maxVolume.Set.Id));

            var maxReps = sets
                .Where(s => s.Set.ActualReps.HasValue)
                .OrderByDescending(s => s.Set.ActualReps!.Value)
                .ThenByDescending(s => s.Set.ActualLoadValue ?? 0)
                .FirstOrDefault();
            if (maxReps != null)
                results.Add(new PersonalRecordItemResponse(
                    exGroup.Key, exName, PersonalRecordType.MaxReps,
                    maxReps.Set.ActualReps!.Value,
                    "reps",
                    maxReps.Set.ActualReps.Value,
                    maxReps.StartedAt ?? DateTimeOffset.UtcNow,
                    maxReps.Id,
                    maxReps.Set.Id));

            var maxDist = sets
                .Where(s => s.Set.DistanceKm.HasValue)
                .OrderByDescending(s => s.Set.DistanceKm!.Value)
                .FirstOrDefault();
            if (maxDist != null)
                results.Add(new PersonalRecordItemResponse(
                    exGroup.Key, exName, PersonalRecordType.MaxDistance,
                    Math.Round(maxDist.Set.DistanceKm!.Value, 3),
                    "km",
                    null,
                    maxDist.StartedAt ?? DateTimeOffset.UtcNow,
                    maxDist.Id,
                    maxDist.Set.Id));

            var maxDur = sets
                .Where(s => s.Set.ActualDuration.HasValue)
                .OrderByDescending(s => s.Set.ActualDuration!.Value.TotalSeconds)
                .FirstOrDefault();
            if (maxDur != null)
                results.Add(new PersonalRecordItemResponse(
                    exGroup.Key, exName, PersonalRecordType.MaxDuration,
                    (decimal)maxDur.Set.ActualDuration!.Value.TotalSeconds,
                    "s",
                    null,
                    maxDur.StartedAt ?? DateTimeOffset.UtcNow,
                    maxDur.Id,
                    maxDur.Set.Id));
        }

        return results;
    }
}
