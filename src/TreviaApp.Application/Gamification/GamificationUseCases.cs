using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Gamification.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Gamification;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Application.Gamification;

#region ====================  SERVICE INTERFACES  ====================

public interface IPointAwardService
{
    Task<AwardWorkoutPointsResultResponse> AwardWorkoutPointsAsync(
        Guid userId,
        Guid workoutSessionId,
        CancellationToken ct);

    Task<int> AddPointsAsync(
        Guid userId,
        int amount,
        PointReason reason,
        string? referenceType = null,
        Guid? referenceId = null,
        string? description = null,
        bool applyDailyCaps = true,
        CancellationToken ct = default);
}

public interface IAchievementEvaluator
{
    Task<List<string>> EvaluateAndUnlockAsync(Guid userId, CancellationToken ct);
    Task<double> CalculateProgressAsync(Guid userId, AchievementDefinition def, CancellationToken ct);
}

public interface IStreakCalculator
{
    Task<UserStreak> RecalculateFromHistoryAsync(Guid userId, CancellationToken ct);
    Task<(bool HasNew7Day, bool HasNew30Day)> ApplyDailyActivityAsync(Guid userId, DateOnly activityDate, CancellationToken ct);
}

public interface IMissionProgressTracker
{
    Task<List<string>> UpdateProgressForWorkoutAsync(Guid userId, Guid workoutSessionId, CancellationToken ct);
    Task IncrementMetricAsync(Guid userId, MissionMetric metric, int amount, CancellationToken ct);
}

#endregion

#region ====================  COMMANDS  ====================

public sealed record AwardWorkoutPointsCommand(
    Guid CurrentUserId,
    bool IsAdmin,
    Guid SessionId)
    : ICommand<AwardWorkoutPointsResultResponse>;

public sealed record AdjustPointsCommand(
    Guid AdminUserId,
    Guid TargetUserId,
    int Amount,
    string Description)
    : ICommand<int>;

public sealed record ClaimMissionCommand(
    Guid CurrentUserId,
    Guid MissionId,
    string Type,
    DateTime? Date = null)
    : ICommand<ClaimMissionResultResponse>;

public sealed record RecomputeStreaksCommand(
    Guid CurrentUserId)
    : ICommand<RecomputeStreaksResultResponse>;

#endregion

#region ====================  VALIDATORS  ====================

public sealed class AwardWorkoutPointsCommandValidator : AbstractValidator<AwardWorkoutPointsCommand>
{
    public AwardWorkoutPointsCommandValidator()
    {
        RuleFor(c => c.CurrentUserId).NotEmpty();
        RuleFor(c => c.SessionId).NotEmpty();
    }
}

public sealed class AdjustPointsCommandValidator : AbstractValidator<AdjustPointsCommand>
{
    public AdjustPointsCommandValidator()
    {
        RuleFor(c => c.AdminUserId).NotEmpty();
        RuleFor(c => c.TargetUserId).NotEmpty();
        RuleFor(c => c.Amount)
            .NotEqual(0).WithMessage("Quantidade de pontos não pode ser zero.")
            .WithErrorCode(ErrorCodes.GamificationInvalidAdjustment);
        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Descrição obrigatória para ajuste manual.")
            .WithErrorCode(ErrorCodes.GamificationInvalidAdjustment)
            .MaximumLength(500);
    }
}

public sealed class ClaimMissionCommandValidator : AbstractValidator<ClaimMissionCommand>
{
    public ClaimMissionCommandValidator()
    {
        RuleFor(c => c.CurrentUserId).NotEmpty();
        RuleFor(c => c.MissionId).NotEmpty();
        RuleFor(c => c.Type)
            .NotEmpty()
            .Must(t => t.Equals("Daily", StringComparison.OrdinalIgnoreCase) ||
                       t.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Tipo deve ser 'Daily' ou 'Weekly'.")
            .WithErrorCode(ErrorCodes.GamificationMissionNotFound);
    }
}

public sealed class RecomputeStreaksCommandValidator : AbstractValidator<RecomputeStreaksCommand>
{
    public RecomputeStreaksCommandValidator()
    {
        RuleFor(c => c.CurrentUserId).NotEmpty();
    }
}

#endregion

#region ====================  QUERIES  ====================

public sealed record GetPointHistoryQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null,
    int Page = 1,
    int PageSize = 20,
    PointReason? Reason = null)
    : IQuery<PaginatedResponse<PointHistoryResponse>>;

public sealed record GetPointBalanceQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null)
    : IQuery<PointBalanceResponse>;

public sealed record GetUserLevelProgressQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null)
    : IQuery<UserLevelProgressResponse>;

public sealed record GetAchievementsWithProgressQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null,
    int Page = 1,
    int PageSize = 50)
    : IQuery<PaginatedResponse<AchievementProgressResponse>>;

public sealed record GetRecentAchievementsQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null,
    int Top = 5)
    : IQuery<List<AchievementProgressResponse>>;

public sealed record GetStreaksSummaryQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null)
    : IQuery<StreaksSummaryResponse>;

public sealed record GetTodayMissionsQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null,
    DateTime? Date = null)
    : IQuery<List<UserMissionProgressResponse>>;

public sealed record GetThisWeekMissionsQuery(
    Guid CurrentUserId,
    bool IsAdminOrCoach,
    Guid? ForUserId = null,
    DateTime? WeekStart = null)
    : IQuery<List<UserMissionProgressResponse>>;

public sealed record GetGamificationDashboardQuery(
    Guid CurrentUserId)
    : IQuery<GamificationDashboardResponse>;

public sealed record GetStudentGamificationDashboardQuery(
    Guid CoachOrAdminUserId,
    bool IsAdmin,
    Guid StudentId)
    : IQuery<GamificationDashboardResponse>;

#endregion

#region ====================  HANDLERS - COMMANDS  ====================

public sealed class AwardWorkoutPointsCommandHandler
    : IRequestHandler<AwardWorkoutPointsCommand, AwardWorkoutPointsResultResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPointAwardService _pointAwardService;
    private readonly IStreakCalculator _streakCalculator;
    private readonly IAchievementEvaluator _achievementEvaluator;
    private readonly IMissionProgressTracker _missionTracker;

    public AwardWorkoutPointsCommandHandler(
        IApplicationDbContext db,
        IPointAwardService pointAwardService,
        IStreakCalculator streakCalculator,
        IAchievementEvaluator achievementEvaluator,
        IMissionProgressTracker missionTracker)
    {
        _db = db;
        _pointAwardService = pointAwardService;
        _streakCalculator = streakCalculator;
        _achievementEvaluator = achievementEvaluator;
        _missionTracker = missionTracker;
    }

    public async Task<AwardWorkoutPointsResultResponse> Handle(AwardWorkoutPointsCommand c, CancellationToken ct)
    {
        var session = await _db.Set<WorkoutSession>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == c.SessionId, ct);

        if (session == null)
            throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);

        if (!c.IsAdmin && session.StudentId != c.CurrentUserId)
            throw new DomainException("Você não tem permissão para pontuar esta sessão.", ErrorCodes.Forbidden);

        if (session.Status != WorkoutStatus.Completed)
            throw new DomainException("Sessão não foi finalizada.", ErrorCodes.WorkoutSessionAlreadyFinished);

        var alreadyAwarded = await _db.Set<PointTransaction>()
            .AsNoTracking()
            .AnyAsync(pt =>
                pt.UserId == session.StudentId &&
                pt.ReferenceType == "WorkoutSession" &&
                pt.ReferenceId == session.Id &&
                pt.Reason == PointReason.WorkoutCompleted, ct);

        if (alreadyAwarded)
        {
            return new AwardWorkoutPointsResultResponse
            {
                Success = false,
                Warning = "Esta sessão já foi pontuada anteriormente.",
            };
        }

        var result = await _pointAwardService.AwardWorkoutPointsAsync(session.StudentId, session.Id, ct);

        if (session.FinishedAt.HasValue)
        {
            DateOnly finishedDate = DateOnly.FromDateTime(session.FinishedAt.Value.DateTime);
            var (hasNew7, hasNew30) = await _streakCalculator.ApplyDailyActivityAsync(session.StudentId, finishedDate, ct);
        }

        var unlockedAchievements = await _achievementEvaluator.EvaluateAndUnlockAsync(session.StudentId, ct);

        var completedMissions = await _missionTracker.UpdateProgressForWorkoutAsync(session.StudentId, session.Id, ct);

        await _db.SaveChangesAsync(ct);

        result = new AwardWorkoutPointsResultResponse
        {
            Success = result.Success,
            PointsEarned = result.PointsEarned,
            XpEarned = result.XpEarned,
            LeveledUp = result.LeveledUp,
            NewLevel = result.NewLevel,
            UnlockedAchievements = unlockedAchievements,
            CompletedMissions = completedMissions,
            Warning = result.Warning,
        };
        return result;
    }
}

public sealed class AdjustPointsCommandHandler
    : IRequestHandler<AdjustPointsCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IPointAwardService _pointAwardService;

    public AdjustPointsCommandHandler(IApplicationDbContext db, IPointAwardService pointAwardService)
    {
        _db = db;
        _pointAwardService = pointAwardService;
    }

    public async Task<int> Handle(AdjustPointsCommand c, CancellationToken ct)
    {
        _ = await _db.Set<AppUser>().FirstOrDefaultAsync(u => u.Id == c.TargetUserId, ct)
            ?? throw new DomainException("Usuário alvo não encontrado.", ErrorCodes.CoachUserNotFound);

        int points = await _pointAwardService.AddPointsAsync(
            c.TargetUserId,
            c.Amount,
            PointReason.ManualAdjustment,
            description: c.Description,
            applyDailyCaps: false,
            ct: ct);

        await _db.SaveChangesAsync(ct);
        return points;
    }
}

public sealed class ClaimMissionCommandHandler
    : IRequestHandler<ClaimMissionCommand, ClaimMissionResultResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPointAwardService _pointAwardService;

    public ClaimMissionCommandHandler(IApplicationDbContext db, IPointAwardService pointAwardService)
    {
        _db = db;
        _pointAwardService = pointAwardService;
    }

    public async Task<ClaimMissionResultResponse> Handle(ClaimMissionCommand c, CancellationToken ct)
    {
        bool isDaily = c.Type.Equals("Daily", StringComparison.OrdinalIgnoreCase);
        DateOnly targetDate = c.Date.HasValue ? DateOnly.FromDateTime(c.Date.Value) : DateOnly.FromDateTime(DateTime.UtcNow);

        if (isDaily)
        {
            var userMission = await _db.Set<UserDailyMission>()
                .Include(m => m.Mission)
                .FirstOrDefaultAsync(m =>
                    m.UserId == c.CurrentUserId &&
                    m.MissionId == c.MissionId &&
                    m.Date == targetDate, ct);

            if (userMission == null)
                throw new DomainException("Missão diária não encontrada.", ErrorCodes.GamificationMissionNotFound);

            if (!userMission.IsCompleted)
                return new ClaimMissionResultResponse
                {
                    Success = false,
                    Error = "Missão ainda não foi completada."
                };

            if (userMission.IsClaimed)
                throw new DomainException("Recompensa já foi reivindicada.", ErrorCodes.GamificationAlreadyClaimed);

            var (points, xp) = userMission.ClaimReward();

            if (points > 0)
            {
                await _pointAwardService.AddPointsAsync(
                    c.CurrentUserId, points, PointReason.DailyMissionCompleted,
                    "UserDailyMission", userMission.Id,
                    $"Missão diária: {userMission.Mission.Title}",
                    applyDailyCaps: false, ct: ct);
            }

            var userLevel = await EnsureUserLevelAsync(c.CurrentUserId, ct);
            if (xp > 0)
            {
                var levelResult = userLevel.AddXp(xp * GamificationConstants.XpPerPoint);
                if (levelResult.LeveledUp && levelResult.BonusPoints > 0)
                {
                    await _pointAwardService.AddPointsAsync(
                        c.CurrentUserId, (int)levelResult.BonusPoints, PointReason.LevelUp,
                        description: $"Subiu para o nível {levelResult.NewLevel}",
                        applyDailyCaps: false, ct: ct);
                }
            }
        }
        else
        {
            DateTime weekStartDt = c.Date.HasValue ? c.Date.Value : DateTime.UtcNow;
            int diff = (int)weekStartDt.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            DateOnly weekStart = DateOnly.FromDateTime(weekStartDt.AddDays(-diff));

            var userMission = await _db.Set<UserWeeklyMission>()
                .Include(m => m.Mission)
                .FirstOrDefaultAsync(m =>
                    m.UserId == c.CurrentUserId &&
                    m.MissionId == c.MissionId &&
                    m.WeekStart == weekStart, ct);

            if (userMission == null)
                throw new DomainException("Missão semanal não encontrada.", ErrorCodes.GamificationMissionNotFound);

            if (!userMission.IsCompleted)
                return new ClaimMissionResultResponse
                {
                    Success = false,
                    Error = "Missão ainda não foi completada."
                };

            if (userMission.IsClaimed)
                throw new DomainException("Recompensa já foi reivindicada.", ErrorCodes.GamificationAlreadyClaimed);

            var (points, xp) = userMission.ClaimReward();

            if (points > 0)
            {
                await _pointAwardService.AddPointsAsync(
                    c.CurrentUserId, points, PointReason.WeeklyMissionCompleted,
                    "UserWeeklyMission", userMission.Id,
                    $"Missão semanal: {userMission.Mission.Title}",
                    applyDailyCaps: false, ct: ct);
            }

            var userLevel = await EnsureUserLevelAsync(c.CurrentUserId, ct);
            if (xp > 0)
            {
                var levelResult = userLevel.AddXp(xp * GamificationConstants.XpPerPoint);
                if (levelResult.LeveledUp && levelResult.BonusPoints > 0)
                {
                    await _pointAwardService.AddPointsAsync(
                        c.CurrentUserId, (int)levelResult.BonusPoints, PointReason.LevelUp,
                        description: $"Subiu para o nível {levelResult.NewLevel}",
                        applyDailyCaps: false, ct: ct);
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        return new ClaimMissionResultResponse { Success = true };
    }

    private async Task<UserLevel> EnsureUserLevelAsync(Guid userId, CancellationToken ct)
    {
        var ul = await _db.Set<UserLevel>().FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (ul == null)
        {
            ul = new UserLevel(userId);
            _db.Set<UserLevel>().Add(ul);
            await _db.SaveChangesAsync(ct);
        }
        return ul;
    }
}

public sealed class RecomputeStreaksCommandHandler
    : IRequestHandler<RecomputeStreaksCommand, RecomputeStreaksResultResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IStreakCalculator _streakCalculator;

    public RecomputeStreaksCommandHandler(IApplicationDbContext db, IStreakCalculator streakCalculator)
    {
        _db = db;
        _streakCalculator = streakCalculator;
    }

    public async Task<RecomputeStreaksResultResponse> Handle(RecomputeStreaksCommand c, CancellationToken ct)
    {
        var streak = await _streakCalculator.RecalculateFromHistoryAsync(c.CurrentUserId, ct);
        await _db.SaveChangesAsync(ct);

        return new RecomputeStreaksResultResponse
        {
            Success = true,
            NewDailyStreak = streak.DailyCurrent,
            NewWeeklyStreak = streak.WeeklyCurrent,
            DailyLongest = streak.DailyLongest,
            WeeklyLongest = streak.WeeklyLongest
        };
    }
}

#endregion

#region ====================  HANDLERS - QUERIES  ====================

public sealed class GetPointHistoryQueryHandler
    : IRequestHandler<GetPointHistoryQuery, PaginatedResponse<PointHistoryResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetPointHistoryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedResponse<PointHistoryResponse>> Handle(GetPointHistoryQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;

        var query = _db.Set<PointTransaction>().AsNoTracking().Where(p => p.UserId == userId);
        if (q.Reason.HasValue)
            query = query.Where(p => p.Reason == q.Reason.Value);

        int totalCount = await query.CountAsync(ct);

        int skip = Math.Max(0, (q.Page - 1) * q.PageSize);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(q.PageSize)
            .Select(p => new PointHistoryResponse
            {
                Id = p.Id,
                Amount = p.Amount,
                Reason = p.Reason,
                ReferenceType = p.ReferenceType,
                ReferenceId = p.ReferenceId,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return new PaginatedResponse<PointHistoryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = q.Page,
            PageSize = q.PageSize,
            HasNextPage = skip + q.PageSize < totalCount
        };
    }
}

public sealed class GetPointBalanceQueryHandler
    : IRequestHandler<GetPointBalanceQuery, PointBalanceResponse>
{
    private readonly IApplicationDbContext _db;

    public GetPointBalanceQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PointBalanceResponse> Handle(GetPointBalanceQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;

        var all = await _db.Set<PointTransaction>()
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Amount, p.CreatedAt })
            .ToListAsync(ct);

        DateTime today = DateTime.UtcNow.Date;
        DateTime weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if ((int)today.DayOfWeek < (int)DayOfWeek.Monday) weekStart = weekStart.AddDays(-7);
        DateTime monthStart = new DateTime(today.Year, today.Month, 1);

        return new PointBalanceResponse
        {
            TotalPoints = all.Sum(p => p.Amount),
            PointsToday = all.Where(p => p.CreatedAt.Date == today).Sum(p => p.Amount),
            PointsThisWeek = all.Where(p => p.CreatedAt.Date >= weekStart.Date).Sum(p => p.Amount),
            PointsThisMonth = all.Where(p => p.CreatedAt.Date >= monthStart.Date).Sum(p => p.Amount),
        };
    }
}

public sealed class GetUserLevelProgressQueryHandler
    : IRequestHandler<GetUserLevelProgressQuery, UserLevelProgressResponse>
{
    private readonly IApplicationDbContext _db;

    public GetUserLevelProgressQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<UserLevelProgressResponse> Handle(GetUserLevelProgressQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;

        var ul = await _db.Set<UserLevel>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        ul ??= new UserLevel(userId);

        return new UserLevelProgressResponse
        {
            CurrentLevel = ul.CurrentLevel,
            CurrentXp = ul.CurrentXp,
            XpToNextLevel = ul.XpToNextLevel(),
            TotalXpEarned = ul.TotalXpEarned,
            ProgressPercentage = ul.ProgressPercentageToNextLevel()
        };
    }
}

public sealed class GetAchievementsWithProgressQueryHandler
    : IRequestHandler<GetAchievementsWithProgressQuery, PaginatedResponse<AchievementProgressResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAchievementEvaluator _evaluator;

    public GetAchievementsWithProgressQueryHandler(IApplicationDbContext db, IAchievementEvaluator evaluator)
    {
        _db = db;
        _evaluator = evaluator;
    }

    public async Task<PaginatedResponse<AchievementProgressResponse>> Handle(GetAchievementsWithProgressQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;

        var defs = await _db.Set<AchievementDefinition>()
            .AsNoTracking()
            .Where(d => d.IsActive && !d.IsDeleted)
            .OrderBy(d => d.Code)
            .ToListAsync(ct);

        var userAch = await _db.Set<UserAchievement>()
            .AsNoTracking()
            .Where(ua => ua.UserId == userId && !ua.IsDeleted)
            .ToDictionaryAsync(ua => ua.AchievementDefinitionId, ct);

        var items = new List<AchievementProgressResponse>();
        foreach (var def in defs)
        {
            var ua = userAch.GetValueOrDefault(def.Id);
            double progress = ua?.Progress ?? 0.0;
            if (ua == null || (!ua.IsUnlocked && progress < 100))
            {
                progress = await _evaluator.CalculateProgressAsync(userId, def, ct);
            }

            items.Add(new AchievementProgressResponse
            {
                AchievementDefinitionId = def.Id,
                Code = def.Code,
                Name = def.Name,
                Description = def.Description,
                Icon = def.Icon,
                PointsReward = def.PointsReward,
                Category = def.Category,
                Progress = progress,
                IsUnlocked = ua?.IsUnlocked ?? false,
                UnlockedAt = ua?.UnlockedAt
            });
        }

        int totalCount = items.Count;
        int skip = Math.Max(0, (q.Page - 1) * q.PageSize);
        var paged = items.Skip(skip).Take(q.PageSize).ToList();

        return new PaginatedResponse<AchievementProgressResponse>
        {
            Items = paged,
            TotalCount = totalCount,
            PageIndex = q.Page,
            PageSize = q.PageSize,
            HasNextPage = skip + q.PageSize < totalCount
        };
    }
}

public sealed class GetRecentAchievementsQueryHandler
    : IRequestHandler<GetRecentAchievementsQuery, List<AchievementProgressResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetRecentAchievementsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<AchievementProgressResponse>> Handle(GetRecentAchievementsQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;

        var items = await _db.Set<UserAchievement>()
            .AsNoTracking()
            .Include(ua => ua.AchievementDefinition)
            .Where(ua => ua.UserId == userId && ua.UnlockedAt.HasValue && !ua.IsDeleted)
            .OrderByDescending(ua => ua.UnlockedAt!.Value)
            .Take(q.Top)
            .Select(ua => new AchievementProgressResponse
            {
                AchievementDefinitionId = ua.AchievementDefinitionId,
                Code = ua.AchievementDefinition.Code,
                Name = ua.AchievementDefinition.Name,
                Description = ua.AchievementDefinition.Description,
                Icon = ua.AchievementDefinition.Icon,
                PointsReward = ua.AchievementDefinition.PointsReward,
                Category = ua.AchievementDefinition.Category,
                Progress = 100.0,
                IsUnlocked = true,
                UnlockedAt = ua.UnlockedAt
            })
            .ToListAsync(ct);

        return items;
    }
}

public sealed class GetStreaksSummaryQueryHandler
    : IRequestHandler<GetStreaksSummaryQuery, StreaksSummaryResponse>
{
    private readonly IApplicationDbContext _db;

    public GetStreaksSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<StreaksSummaryResponse> Handle(GetStreaksSummaryQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;

        var s = await _db.Set<UserStreak>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        return new StreaksSummaryResponse
        {
            DailyCurrent = s?.DailyCurrent ?? 0,
            DailyLongest = s?.DailyLongest ?? 0,
            DailyLastActiveAt = s?.DailyLastActiveAt,
            WeeklyCurrent = s?.WeeklyCurrent ?? 0,
            WeeklyLongest = s?.WeeklyLongest ?? 0,
            WeekStartDate = s?.WeekStartDate
        };
    }
}

public sealed class GetTodayMissionsQueryHandler
    : IRequestHandler<GetTodayMissionsQuery, List<UserMissionProgressResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetTodayMissionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<UserMissionProgressResponse>> Handle(GetTodayMissionsQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;
        DateOnly date = q.Date.HasValue ? DateOnly.FromDateTime(q.Date.Value) : DateOnly.FromDateTime(DateTime.UtcNow);

        var defs = await _db.Set<DailyMissionDefinition>()
            .AsNoTracking()
            .Where(d => d.IsActive && !d.IsDeleted)
            .ToListAsync(ct);

        var userMissions = await _db.Set<UserDailyMission>()
            .AsNoTracking()
            .Include(m => m.Mission)
            .Where(m => m.UserId == userId && m.Date == date && !m.IsDeleted)
            .ToDictionaryAsync(m => m.MissionId, ct);

        var result = new List<UserMissionProgressResponse>();
        foreach (var def in defs)
        {
            var um = userMissions.GetValueOrDefault(def.Id);
            result.Add(new UserMissionProgressResponse
            {
                MissionId = def.Id,
                Code = def.Code,
                Title = def.Title,
                Description = def.Description,
                TargetValue = def.TargetValue,
                Metric = def.Metric,
                PointsReward = def.PointsReward,
                XpReward = def.XpReward,
                CurrentValue = um?.CurrentValue ?? 0,
                ProgressPercentage = def.TargetValue > 0 ? Math.Min(100.0, (double)(um?.CurrentValue ?? 0) / def.TargetValue * 100.0) : 0,
                IsCompleted = um?.IsCompleted ?? false,
                CompletedAt = um?.CompletedAt,
                IsClaimed = um?.IsClaimed ?? false,
                ClaimedAt = um?.ClaimedAt
            });
        }

        return result;
    }
}

public sealed class GetThisWeekMissionsQueryHandler
    : IRequestHandler<GetThisWeekMissionsQuery, List<UserMissionProgressResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetThisWeekMissionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<UserMissionProgressResponse>> Handle(GetThisWeekMissionsQuery q, CancellationToken ct)
    {
        Guid userId = q.ForUserId ?? q.CurrentUserId;
        DateTime dt = q.WeekStart ?? DateTime.UtcNow;
        int diff = (int)dt.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        DateOnly weekStart = DateOnly.FromDateTime(dt.AddDays(-diff));

        var defs = await _db.Set<WeeklyMissionDefinition>()
            .AsNoTracking()
            .Where(d => d.IsActive && !d.IsDeleted)
            .ToListAsync(ct);

        var userMissions = await _db.Set<UserWeeklyMission>()
            .AsNoTracking()
            .Include(m => m.Mission)
            .Where(m => m.UserId == userId && m.WeekStart == weekStart && !m.IsDeleted)
            .ToDictionaryAsync(m => m.MissionId, ct);

        var result = new List<UserMissionProgressResponse>();
        foreach (var def in defs)
        {
            var um = userMissions.GetValueOrDefault(def.Id);
            result.Add(new UserMissionProgressResponse
            {
                MissionId = def.Id,
                Code = def.Code,
                Title = def.Title,
                Description = def.Description,
                TargetValue = def.TargetValue,
                Metric = def.Metric,
                PointsReward = def.PointsReward,
                XpReward = def.XpReward,
                CurrentValue = um?.CurrentValue ?? 0,
                ProgressPercentage = def.TargetValue > 0 ? Math.Min(100.0, (double)(um?.CurrentValue ?? 0) / def.TargetValue * 100.0) : 0,
                IsCompleted = um?.IsCompleted ?? false,
                CompletedAt = um?.CompletedAt,
                IsClaimed = um?.IsClaimed ?? false,
                ClaimedAt = um?.ClaimedAt
            });
        }

        return result;
    }
}

public sealed class GetGamificationDashboardQueryHandler
    : IRequestHandler<GetGamificationDashboardQuery, GamificationDashboardResponse>
{
    private readonly IMediator _mediator;

    public GetGamificationDashboardQueryHandler(IMediator mediator) => _mediator = mediator;

    public async Task<GamificationDashboardResponse> Handle(GetGamificationDashboardQuery q, CancellationToken ct)
    {
        var level = await _mediator.Send(new GetUserLevelProgressQuery(q.CurrentUserId, false), ct);
        var balance = await _mediator.Send(new GetPointBalanceQuery(q.CurrentUserId, false), ct);
        var streaks = await _mediator.Send(new GetStreaksSummaryQuery(q.CurrentUserId, false), ct);
        var recentAch = await _mediator.Send(new GetRecentAchievementsQuery(q.CurrentUserId, false, null, 3), ct);
        var allAch = await _mediator.Send(new GetAchievementsWithProgressQuery(q.CurrentUserId, false, null, 1, 100), ct);
        var todayMissions = await _mediator.Send(new GetTodayMissionsQuery(q.CurrentUserId, false), ct);
        var history = await _mediator.Send(new GetPointHistoryQuery(q.CurrentUserId, false, null, 1, 5), ct);

        var nextAchievements = allAch.Items
            .Where(a => !a.IsUnlocked)
            .OrderByDescending(a => a.Progress)
            .Take(3)
            .ToList();

        return new GamificationDashboardResponse
        {
            CurrentLevel = level.CurrentLevel,
            CurrentXp = level.CurrentXp,
            XpToNextLevel = level.XpToNextLevel,
            XpProgressPercentage = level.ProgressPercentage,
            TotalPoints = balance.TotalPoints,
            Streaks = streaks,
            NextAchievements = nextAchievements,
            RecentAchievements = recentAch,
            TodayMissions = todayMissions,
            RecentTransactions = history.Items
        };
    }
}

public sealed class GetStudentGamificationDashboardQueryHandler
    : IRequestHandler<GetStudentGamificationDashboardQuery, GamificationDashboardResponse>
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public GetStudentGamificationDashboardQueryHandler(IMediator mediator, IApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    public async Task<GamificationDashboardResponse> Handle(GetStudentGamificationDashboardQuery q, CancellationToken ct)
    {
        if (!q.IsAdmin)
        {
            var link = await _db.Set<Domain.Coaching.CoachStudentLink>()
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.CoachId == q.CoachOrAdminUserId &&
                    l.StudentId == q.StudentId &&
                    l.IsActive && !l.IsDeleted, ct);

            if (link == null || !link.HasPermission(CoachPermissions.CanViewWorkoutHistory))
                throw new DomainException("Sem permissão para ver dados do aluno.", ErrorCodes.Forbidden);
        }

        return await _mediator.Send(new GetGamificationDashboardQuery(q.StudentId), ct);
    }
}

#endregion
