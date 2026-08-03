namespace TreviaApp.Infrastructure.Gamification;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Gamification;
using TreviaApp.Contracts.Gamification.Responses;
using TreviaApp.Domain.Gamification;
using TreviaApp.Domain.Identity;
using TreviaApp.Infrastructure.Persistence;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public class PointAwardService : IPointAwardService
{
    private readonly ApplicationDbContext _db;

    public PointAwardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AwardWorkoutPointsResultResponse> AwardWorkoutPointsAsync(
        Guid userId,
        Guid workoutSessionId,
        CancellationToken ct)
    {
        var response = new AwardWorkoutPointsResultResponse();

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int workoutsToday = await _db.Set<PointTransaction>()
            .AsNoTracking()
            .CountAsync(pt =>
                pt.UserId == userId &&
                pt.Reason == PointReason.WorkoutCompleted &&
                pt.CreatedAt.Date == today.ToDateTime(TimeOnly.MinValue).Date, ct);

        int workoutPoints = 0;
        if (workoutsToday < GamificationConstants.DailyWorkoutAwardCap)
        {
            workoutPoints = GamificationConstants.WorkoutCompletedPoints;
        }
        else
        {
            response.Warning = $"Limite diário de {GamificationConstants.DailyWorkoutAwardCap} treino(s) para pontuação já atingido.";
        }

        int setsCompletedPoints = 0;
        var sessionSets = await _db.Set<Domain.WorkoutExecution.WorkoutSet>()
            .AsNoTracking()
            .Include(s => s.WorkoutExercise)
            .Where(s => s.WorkoutExercise!.WorkoutSessionId == workoutSessionId && s.Completed)
            .ToListAsync(ct);

        int setCount = sessionSets.Count;
        int setPointsToday = await _db.Set<PointTransaction>()
            .AsNoTracking()
            .Where(pt =>
                pt.UserId == userId &&
                pt.Reason == PointReason.SetCompleted &&
                pt.CreatedAt.Date == today.ToDateTime(TimeOnly.MinValue).Date)
            .SumAsync(pt => pt.Amount, ct);

        int availableSetPoints = Math.Max(0, GamificationConstants.DailySetPointsCap - setPointsToday);
        int rawSetPoints = setCount * GamificationConstants.SetCompletedPoints;
        setsCompletedPoints = Math.Min(availableSetPoints, rawSetPoints);

        int totalPoints = workoutPoints + setsCompletedPoints;
        response.PointsEarned = totalPoints;

        if (workoutPoints > 0)
        {
            _db.Set<PointTransaction>().Add(new PointTransaction(
                userId, workoutPoints, PointReason.WorkoutCompleted,
                "WorkoutSession", workoutSessionId,
                "Treino concluído com sucesso"));
        }

        if (setsCompletedPoints > 0)
        {
            _db.Set<PointTransaction>().Add(new PointTransaction(
                userId, setsCompletedPoints, PointReason.SetCompleted,
                "WorkoutSession", workoutSessionId,
                $"{Math.Min(setCount, setsCompletedPoints)} séries concluídas"));
        }

        if (totalPoints > 0)
        {
            long xpToAdd = totalPoints * GamificationConstants.XpPerPoint;
            var userLevel = await EnsureUserLevelAsync(userId, ct);
            var (leveledUp, newLevel, bonusPoints) = userLevel.AddXp(xpToAdd);

            response.XpEarned = xpToAdd;
            response.LeveledUp = leveledUp;
            if (leveledUp)
            {
                response.NewLevel = newLevel;
                if (bonusPoints > 0)
                {
                    _db.Set<PointTransaction>().Add(new PointTransaction(
                        userId, (int)bonusPoints, PointReason.LevelUp,
                        description: $"Bônus por subir para nível {newLevel}"));
                    response.PointsEarned += (int)bonusPoints;
                }
            }
        }

        response.Success = true;
        return response;
    }

    public async Task<int> AddPointsAsync(
        Guid userId,
        int amount,
        PointReason reason,
        string? referenceType = null,
        Guid? referenceId = null,
        string? description = null,
        bool applyDailyCaps = true,
        CancellationToken ct = default)
    {
        if (amount == 0) return 0;

        int finalAmount = amount;

        if (applyDailyCaps && amount > 0)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (reason == PointReason.WorkoutCompleted)
            {
                int workoutsToday = await _db.Set<PointTransaction>()
                    .AsNoTracking()
                    .CountAsync(pt =>
                        pt.UserId == userId &&
                        pt.Reason == PointReason.WorkoutCompleted &&
                        pt.CreatedAt.Date == today.ToDateTime(TimeOnly.MinValue).Date, ct);

                if (workoutsToday >= GamificationConstants.DailyWorkoutAwardCap)
                    return 0;
            }

            if (reason == PointReason.SetCompleted)
            {
                int setPointsToday = await _db.Set<PointTransaction>()
                    .AsNoTracking()
                    .Where(pt =>
                        pt.UserId == userId &&
                        pt.Reason == PointReason.SetCompleted &&
                        pt.CreatedAt.Date == today.ToDateTime(TimeOnly.MinValue).Date)
                    .SumAsync(pt => pt.Amount, ct);

                int available = Math.Max(0, GamificationConstants.DailySetPointsCap - setPointsToday);
                finalAmount = Math.Min(available, amount);
                if (finalAmount <= 0) return 0;
            }
        }

        _db.Set<PointTransaction>().Add(new PointTransaction(
            userId, finalAmount, reason, referenceType, referenceId, description));

        if (finalAmount > 0)
        {
            long xpToAdd = finalAmount * GamificationConstants.XpPerPoint;
            var userLevel = await EnsureUserLevelAsync(userId, ct);
            var (leveledUp, newLevel, bonusPoints) = userLevel.AddXp(xpToAdd);

            if (leveledUp && bonusPoints > 0)
            {
                _db.Set<PointTransaction>().Add(new PointTransaction(
                    userId, (int)bonusPoints, PointReason.LevelUp,
                    description: $"Bônus por subir para nível {newLevel}"));
                finalAmount += (int)bonusPoints;
            }
        }

        return finalAmount;
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

public class AchievementEvaluator : IAchievementEvaluator
{
    private readonly ApplicationDbContext _db;

    public AchievementEvaluator(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<string>> EvaluateAndUnlockAsync(Guid userId, CancellationToken ct)
    {
        var unlocked = new List<string>();
        var defs = await _db.Set<AchievementDefinition>()
            .AsNoTracking()
            .Where(d => d.IsActive && !d.IsDeleted)
            .ToListAsync(ct);

        foreach (var def in defs)
        {
            var ua = await _db.Set<UserAchievement>()
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementDefinitionId == def.Id, ct);

            if (ua == null)
            {
                ua = new UserAchievement(userId, def.Id);
                _db.Set<UserAchievement>().Add(ua);
            }

            if (ua.IsUnlocked) continue;

            double progress = await CalculateProgressInternal(userId, def, ct);
            ua.UpdateProgress(progress);

            if (progress >= 100.0)
            {
                ua.Unlock();
                unlocked.Add(def.Code);

                if (def.PointsReward > 0)
                {
                    _db.Set<PointTransaction>().Add(new PointTransaction(
                        userId, def.PointsReward, PointReason.AchievementUnlocked,
                        "AchievementDefinition", def.Id,
                        $"Conquista desbloqueada: {def.Name}"));
                }
            }
        }

        return unlocked;
    }

    public async Task<double> CalculateProgressAsync(Guid userId, AchievementDefinition def, CancellationToken ct)
    {
        return await CalculateProgressInternal(userId, def, ct);
    }

    private async Task<double> CalculateProgressInternal(Guid userId, AchievementDefinition def, CancellationToken ct)
    {
        switch (def.Code)
        {
            case GamificationConstants.AchievementCodes.AC001:
                {
                    int count = await _db.Set<Domain.WorkoutExecution.WorkoutSession>()
                        .AsNoTracking()
                        .CountAsync(s => s.StudentId == userId && s.Status == WorkoutStatus.Completed, ct);
                    return Math.Min(100.0, count * 100.0);
                }
            case GamificationConstants.AchievementCodes.AC002:
                {
                    int count = await _db.Set<Domain.WorkoutExecution.WorkoutSession>()
                        .AsNoTracking()
                        .CountAsync(s => s.StudentId == userId && s.Status == WorkoutStatus.Completed, ct);
                    return Math.Min(100.0, count / 10.0 * 100.0);
                }
            case GamificationConstants.AchievementCodes.AC003:
            case GamificationConstants.AchievementCodes.AC004:
                {
                    var streak = await _db.Set<UserStreak>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.UserId == userId, ct);
                    int current = streak?.DailyCurrent ?? 0;
                    int target = def.Code == GamificationConstants.AchievementCodes.AC003 ? 7 : 30;
                    return Math.Min(100.0, (double)current / target * 100.0);
                }
            case GamificationConstants.AchievementCodes.AC005:
                {
                    int count = await _db.Set<PointTransaction>()
                        .AsNoTracking()
                        .CountAsync(pt => pt.UserId == userId && pt.Reason == PointReason.WorkoutCompleted, ct);
                    return Math.Min(100.0, count >= 1 ? 100.0 : 0.0);
                }
            case GamificationConstants.AchievementCodes.AC006:
                {
                    int wf = await _db.Set<Domain.WorkoutExecution.Feedback.WorkoutFeedback>()
                        .AsNoTracking()
                        .CountAsync(f => f.StudentId == userId && f.ReadAt != null, ct);
                    int ef = await _db.Set<Domain.WorkoutExecution.Feedback.ExerciseFeedback>()
                        .AsNoTracking()
                        .CountAsync(f => f.StudentId == userId && f.ReadAt != null, ct);
                    int sf = await _db.Set<Domain.WorkoutExecution.Feedback.SetFeedback>()
                        .AsNoTracking()
                        .CountAsync(f => f.StudentId == userId && f.ReadAt != null, ct);
                    int total = wf + ef + sf;
                    return Math.Min(100.0, total / 5.0 * 100.0);
                }
            case GamificationConstants.AchievementCodes.AC007:
                {
                    int count = await _db.Set<Domain.WorkoutExecution.WorkoutSet>()
                        .AsNoTracking()
                        .Include(s => s.WorkoutExercise)
                        .CountAsync(s => s.Completed && s.WorkoutExercise!.WorkoutSession!.StudentId == userId, ct);
                    return Math.Min(100.0, count / 100.0 * 100.0);
                }
            case GamificationConstants.AchievementCodes.AC008:
                {
                    int count = await _db.Set<Domain.TrainingPlans.TrainingPlan>()
                        .AsNoTracking()
                        .CountAsync(tp => tp.AssignedToStudentId == userId && tp.Status == TrainingPlanStatus.Completed, ct);
                    return Math.Min(100.0, count * 100.0);
                }
            case GamificationConstants.AchievementCodes.AC009:
            case GamificationConstants.AchievementCodes.AC010:
                {
                    var ul = await _db.Set<UserLevel>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == userId, ct);
                    int current = ul?.CurrentLevel ?? 1;
                    int target = def.Code == GamificationConstants.AchievementCodes.AC009 ? 5 : 10;
                    return Math.Min(100.0, (double)current / target * 100.0);
                }
            default:
                return 0.0;
        }
    }
}

public class StreakCalculator : IStreakCalculator
{
    private readonly ApplicationDbContext _db;

    public StreakCalculator(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserStreak> RecalculateFromHistoryAsync(Guid userId, CancellationToken ct)
    {
        var userStreak = await _db.Set<UserStreak>().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        userStreak ??= new UserStreak(userId);
        _db.Set<UserStreak>().Update(userStreak);

        var completedDates = await _db.Set<Domain.WorkoutExecution.WorkoutSession>()
            .AsNoTracking()
            .Where(s => s.StudentId == userId &&
                        s.Status == WorkoutStatus.Completed &&
                        s.FinishedAt.HasValue)
            .Select(s => s.FinishedAt!.Value)
            .OrderBy(d => d)
            .ToListAsync(ct);

        var uniqueDates = completedDates
            .Select(d => DateOnly.FromDateTime(d.DateTime))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        userStreak.Reset();

        if (uniqueDates.Count == 0)
            return userStreak;

        int dailyCurrent = 0;
        int dailyLongest = 0;
        DateOnly? lastDate = null;

        foreach (var date in uniqueDates)
        {
            if (!lastDate.HasValue)
            {
                dailyCurrent = 1;
            }
            else
            {
                int diff = date.DayNumber - lastDate.Value.DayNumber;
                if (diff == 1)
                {
                    dailyCurrent++;
                }
                else if (diff > 1)
                {
                    dailyCurrent = 1;
                }
            }

            if (dailyCurrent > dailyLongest)
                dailyLongest = dailyCurrent;

            lastDate = date;
        }

        userStreak.UpdateDaily(lastDate!.Value);
        userStreak.SetDailyLongest(dailyLongest);

        var weeklyGroups = uniqueDates
            .GroupBy(d =>
            {
                int diff = (int)d.DayOfWeek - (int)DayOfWeek.Monday;
                if (diff < 0) diff += 7;
                return d.AddDays(-diff);
            })
            .OrderBy(g => g.Key)
            .ToList();

        if (weeklyGroups.Count > 0)
        {
            var lastWeek = weeklyGroups.Last();
            int weeklyCurrent = lastWeek.Count();
            int weeklyLongest = weeklyGroups.Max(g => g.Count());

            userStreak.UpdateWeekly(lastWeek.Key, weeklyCurrent);
            userStreak.SetWeeklyLongest(weeklyLongest);
        }

        return userStreak;
    }

    public async Task<(bool HasNew7Day, bool HasNew30Day)> ApplyDailyActivityAsync(Guid userId, DateOnly activityDate, CancellationToken ct)
    {
        var userStreak = await _db.Set<UserStreak>().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (userStreak == null)
        {
            userStreak = new UserStreak(userId);
            _db.Set<UserStreak>().Add(userStreak);
        }

        int oldDaily = userStreak.DailyCurrent;
        userStreak.UpdateDaily(activityDate);
        int newDaily = userStreak.DailyCurrent;

        bool hasNew7 = oldDaily < 7 && newDaily >= 7;
        bool hasNew30 = oldDaily < 30 && newDaily >= 30;

        if (hasNew7)
        {
            _db.Set<PointTransaction>().Add(new PointTransaction(
                userId,
                GamificationConstants.Streak7DaysPoints,
                PointReason.Streak7Days,
                description: $"Streak de 7 dias consecutivos alcançado!"));
        }

        if (hasNew30)
        {
            _db.Set<PointTransaction>().Add(new PointTransaction(
                userId,
                GamificationConstants.Streak30DaysPoints,
                PointReason.Streak30Days,
                description: $"Streak de 30 dias consecutivos alcançado!"));
        }

        return (hasNew7, hasNew30);
    }
}

public class MissionProgressTracker : IMissionProgressTracker
{
    private readonly ApplicationDbContext _db;

    public MissionProgressTracker(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<string>> UpdateProgressForWorkoutAsync(Guid userId, Guid workoutSessionId, CancellationToken ct)
    {
        var completed = new List<string>();

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateTime dt = DateTime.UtcNow;
        int diff = (int)dt.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        DateOnly weekStart = DateOnly.FromDateTime(dt.AddDays(-diff));

        var sessionSets = await _db.Set<Domain.WorkoutExecution.WorkoutSet>()
            .AsNoTracking()
            .Include(s => s.WorkoutExercise)
            .CountAsync(s => s.Completed && s.WorkoutExercise!.WorkoutSessionId == workoutSessionId, ct);

        completed.AddRange(await IncrementDailyMetric(userId, today, MissionMetric.WorkoutsCompleted, 1, ct));
        completed.AddRange(await IncrementDailyMetric(userId, today, MissionMetric.SetsCompleted, sessionSets, ct));
        completed.AddRange(await IncrementWeeklyMetric(userId, weekStart, MissionMetric.WorkoutsCompleted, 1, ct));
        completed.AddRange(await IncrementWeeklyMetric(userId, weekStart, MissionMetric.SetsCompleted, sessionSets, ct));

        return completed;
    }

    public async Task IncrementMetricAsync(Guid userId, MissionMetric metric, int amount, CancellationToken ct)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateTime dt = DateTime.UtcNow;
        int diff = (int)dt.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        DateOnly weekStart = DateOnly.FromDateTime(dt.AddDays(-diff));

        await IncrementDailyMetric(userId, today, metric, amount, ct);
        await IncrementWeeklyMetric(userId, weekStart, metric, amount, ct);
    }

    private async Task<List<string>> IncrementDailyMetric(Guid userId, DateOnly date, MissionMetric metric, int amount, CancellationToken ct)
    {
        var completed = new List<string>();
        var defs = await _db.Set<DailyMissionDefinition>()
            .Where(d => d.IsActive && !d.IsDeleted && d.Metric == metric)
            .ToListAsync(ct);

        foreach (var def in defs)
        {
            var um = await _db.Set<UserDailyMission>()
                .FirstOrDefaultAsync(m =>
                    m.UserId == userId && m.MissionId == def.Id && m.Date == date, ct);

            if (um == null)
            {
                um = new UserDailyMission(userId, def.Id, date);
                _db.Set<UserDailyMission>().Add(um);
                await _db.SaveChangesAsync(ct);
                um = await _db.Set<UserDailyMission>()
                    .FirstOrDefaultAsync(m =>
                        m.UserId == userId && m.MissionId == def.Id && m.Date == date, ct);
            }

            bool wasCompleted = um!.IsCompleted;
            um.IncrementProgress(amount);
            if (!wasCompleted && um.IsCompleted)
            {
                completed.Add(def.Code);
            }
        }

        return completed;
    }

    private async Task<List<string>> IncrementWeeklyMetric(Guid userId, DateOnly weekStart, MissionMetric metric, int amount, CancellationToken ct)
    {
        var completed = new List<string>();
        var defs = await _db.Set<WeeklyMissionDefinition>()
            .Where(d => d.IsActive && !d.IsDeleted && d.Metric == metric)
            .ToListAsync(ct);

        foreach (var def in defs)
        {
            var um = await _db.Set<UserWeeklyMission>()
                .FirstOrDefaultAsync(m =>
                    m.UserId == userId && m.MissionId == def.Id && m.WeekStart == weekStart, ct);

            if (um == null)
            {
                um = new UserWeeklyMission(userId, def.Id, weekStart);
                _db.Set<UserWeeklyMission>().Add(um);
                await _db.SaveChangesAsync(ct);
                um = await _db.Set<UserWeeklyMission>()
                    .FirstOrDefaultAsync(m =>
                        m.UserId == userId && m.MissionId == def.Id && m.WeekStart == weekStart, ct);
            }

            bool wasCompleted = um!.IsCompleted;
            um.IncrementProgress(amount);
            if (!wasCompleted && um.IsCompleted)
            {
                completed.Add(def.Code);
            }
        }

        return completed;
    }
}
