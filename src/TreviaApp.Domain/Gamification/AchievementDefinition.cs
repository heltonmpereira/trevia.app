using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Gamification;

public class AchievementDefinition : Entity
{
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public string? Icon { get; private set; }

    public int PointsReward { get; private set; } = 0;

    public AchievementCategory Category { get; private set; }

    public string? CriteriaConfigJson { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<UserAchievement> UserAchievements { get; private set; } = new List<UserAchievement>();

    private AchievementDefinition()
    {
    }

    public AchievementDefinition(
        string code,
        string name,
        string description,
        AchievementCategory category,
        int pointsReward = 0,
        string? icon = null,
        string? criteriaConfigJson = null)
    {
        Code = code;
        Name = name;
        Description = description;
        Category = category;
        PointsReward = pointsReward;
        Icon = icon;
        CriteriaConfigJson = criteriaConfigJson;
        IsActive = true;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
