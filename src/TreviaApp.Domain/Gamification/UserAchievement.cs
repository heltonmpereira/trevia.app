using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;

namespace TreviaApp.Domain.Gamification;

public class UserAchievement : Entity
{
    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public Guid AchievementDefinitionId { get; private set; }

    public AchievementDefinition AchievementDefinition { get; private set; } = null!;

    public DateTimeOffset? UnlockedAt { get; private set; }

    public double Progress { get; private set; } = 0.0;

    public bool IsUnlocked => UnlockedAt.HasValue;

    private UserAchievement()
    {
    }

    public UserAchievement(Guid userId, Guid achievementDefinitionId)
    {
        UserId = userId;
        AchievementDefinitionId = achievementDefinitionId;
        Progress = 0.0;
    }

    public void UpdateProgress(double progress)
    {
        Progress = Math.Clamp(progress, 0.0, 100.0);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unlock()
    {
        if (!IsUnlocked)
        {
            UnlockedAt = DateTimeOffset.UtcNow;
            Progress = 100.0;
            UpdatedAt = UnlockedAt.Value;
        }
    }
}
