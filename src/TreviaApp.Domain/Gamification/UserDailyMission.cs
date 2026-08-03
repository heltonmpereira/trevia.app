using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;

namespace TreviaApp.Domain.Gamification;

public class UserDailyMission : Entity
{
    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public Guid MissionId { get; private set; }

    public DailyMissionDefinition Mission { get; private set; } = null!;

    public DateOnly Date { get; private set; }

    public int CurrentValue { get; private set; } = 0;

    public bool IsCompleted { get; private set; } = false;

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? ClaimedAt { get; private set; }

    public bool IsClaimed => ClaimedAt.HasValue;

    private UserDailyMission()
    {
    }

    public UserDailyMission(Guid userId, Guid missionId, DateOnly date)
    {
        UserId = userId;
        MissionId = missionId;
        Date = date;
    }

    public void IncrementProgress(int amount = 1)
    {
        if (IsCompleted)
        {
            return;
        }

        CurrentValue += amount;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (CurrentValue >= Mission.TargetValue)
        {
            IsCompleted = true;
            CompletedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CompletedAt.Value;
        }
    }

    public (int Points, int Xp) ClaimReward()
    {
        if (!IsCompleted || IsClaimed)
        {
            return (0, 0);
        }

        ClaimedAt = DateTimeOffset.UtcNow;
        UpdatedAt = ClaimedAt.Value;
        return (Mission.PointsReward, Mission.XpReward);
    }
}
