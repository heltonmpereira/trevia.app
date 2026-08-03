using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Gamification;

public class DailyMissionDefinition : Entity
{
    public string Code { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public int TargetValue { get; private set; }

    public MissionMetric Metric { get; private set; }

    public int PointsReward { get; private set; }

    public int XpReward { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<UserDailyMission> UserDailyMissions { get; private set; } = new List<UserDailyMission>();

    private DailyMissionDefinition()
    {
    }

    public DailyMissionDefinition(
        string code,
        string title,
        string description,
        int targetValue,
        MissionMetric metric,
        int pointsReward,
        int xpReward)
    {
        Code = code;
        Title = title;
        Description = description;
        TargetValue = targetValue;
        Metric = metric;
        PointsReward = pointsReward;
        XpReward = xpReward;
        IsActive = true;
    }
}
