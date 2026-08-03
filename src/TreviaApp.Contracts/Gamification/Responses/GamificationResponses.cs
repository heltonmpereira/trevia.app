using TreviaApp.Contracts.Common;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Gamification.Responses;

public sealed record PointHistoryResponse
{
    public PointHistoryResponse() { }

    public Guid Id { get; init; }
    public int Amount { get; init; }
    public PointReason Reason { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record PointBalanceResponse
{
    public PointBalanceResponse() { }

    public int TotalPoints { get; init; }
    public int PointsToday { get; init; }
    public int PointsThisWeek { get; init; }
    public int PointsThisMonth { get; init; }
}

public sealed record UserLevelProgressResponse
{
    public UserLevelProgressResponse() { }

    public int CurrentLevel { get; init; }
    public long CurrentXp { get; init; }
    public long XpToNextLevel { get; init; }
    public long TotalXpEarned { get; init; }
    public double ProgressPercentage { get; init; }
}

public sealed record AchievementProgressResponse
{
    public AchievementProgressResponse() { }

    public Guid AchievementDefinitionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public int PointsReward { get; init; }
    public AchievementCategory Category { get; init; }
    public double Progress { get; init; }
    public bool IsUnlocked { get; init; }
    public DateTimeOffset? UnlockedAt { get; init; }
}

public sealed record StreaksSummaryResponse
{
    public StreaksSummaryResponse() { }

    public int DailyCurrent { get; init; }
    public int DailyLongest { get; init; }
    public DateOnly? DailyLastActiveAt { get; init; }
    public int WeeklyCurrent { get; init; }
    public int WeeklyLongest { get; init; }
    public DateOnly? WeekStartDate { get; init; }
}

public sealed record UserMissionProgressResponse
{
    public UserMissionProgressResponse() { }

    public Guid MissionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int TargetValue { get; init; }
    public MissionMetric Metric { get; init; }
    public int PointsReward { get; init; }
    public int XpReward { get; init; }
    public int CurrentValue { get; init; }
    public double ProgressPercentage { get; init; }
    public bool IsCompleted { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public bool IsClaimed { get; init; }
    public DateTimeOffset? ClaimedAt { get; init; }
}

public sealed record GamificationDashboardResponse
{
    public GamificationDashboardResponse() { }

    public int CurrentLevel { get; init; }
    public long CurrentXp { get; init; }
    public long XpToNextLevel { get; init; }
    public double XpProgressPercentage { get; init; }
    public int TotalPoints { get; init; }

    public StreaksSummaryResponse Streaks { get; init; } = new();

    public List<AchievementProgressResponse> NextAchievements { get; init; } = new();
    public List<AchievementProgressResponse> RecentAchievements { get; init; } = new();

    public List<UserMissionProgressResponse> TodayMissions { get; init; } = new();
    public List<PointHistoryResponse> RecentTransactions { get; init; } = new();
}

public sealed record AwardWorkoutPointsResultResponse
{
    public AwardWorkoutPointsResultResponse() { }

    public bool Success { get; set; }
    public int PointsEarned { get; set; }
    public long XpEarned { get; set; }
    public bool LeveledUp { get; set; }
    public int? NewLevel { get; set; }
    public List<string> UnlockedAchievements { get; set; } = new();
    public List<string> CompletedMissions { get; set; } = new();
    public string? Warning { get; set; }
}

public sealed record ClaimMissionResultResponse
{
    public ClaimMissionResultResponse() { }

    public bool Success { get; init; }
    public int PointsEarned { get; init; }
    public int XpEarned { get; init; }
    public string? Error { get; init; }
}

public sealed record RecomputeStreaksResultResponse
{
    public RecomputeStreaksResultResponse() { }

    public bool Success { get; init; }
    public int NewDailyStreak { get; init; }
    public int NewWeeklyStreak { get; init; }
    public int DailyLongest { get; init; }
    public int WeeklyLongest { get; init; }
}

public sealed class PointHistoryPagedResponse : PaginatedResponse<PointHistoryResponse>
{
}

public sealed class AchievementsPagedResponse : PaginatedResponse<AchievementProgressResponse>
{
}

public sealed class MissionsPagedResponse : PaginatedResponse<UserMissionProgressResponse>
{
}
