using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

namespace TreviaApp.Domain.Gamification;

public class UserLevel : Entity
{
    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public int CurrentLevel { get; private set; } = 1;

    public long CurrentXp { get; private set; } = 0;

    public long TotalXpEarned { get; private set; } = 0;

    private UserLevel()
    {
    }

    public UserLevel(Guid userId)
    {
        UserId = userId;
        CurrentLevel = 1;
        CurrentXp = 0;
        TotalXpEarned = 0;
    }

    public (bool LeveledUp, int NewLevel, long BonusPoints) AddXp(long xpToAdd)
    {
        if (xpToAdd <= 0)
        {
            return (false, CurrentLevel, 0);
        }

        TotalXpEarned += xpToAdd;
        CurrentXp += xpToAdd;

        bool leveledUp = false;
        int newLevel = CurrentLevel;
        long totalBonusPoints = 0;

        while (CurrentLevel < GamificationConstants.MaxLevel)
        {
            long xpRequired = LevelCurve.XpRequiredForLevel(CurrentLevel);
            if (CurrentXp < xpRequired)
            {
                break;
            }

            CurrentXp -= xpRequired;
            CurrentLevel++;
            newLevel = CurrentLevel;
            leveledUp = true;
            totalBonusPoints += CurrentLevel * 50;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return (leveledUp, newLevel, totalBonusPoints);
    }

    public long XpToNextLevel()
    {
        if (CurrentLevel >= GamificationConstants.MaxLevel)
        {
            return 0;
        }

        long required = LevelCurve.XpRequiredForLevel(CurrentLevel);
        return Math.Max(0, required - CurrentXp);
    }

    public double ProgressPercentageToNextLevel()
    {
        if (CurrentLevel >= GamificationConstants.MaxLevel)
        {
            return 100.0;
        }

        long required = LevelCurve.XpRequiredForLevel(CurrentLevel);
        if (required <= 0)
        {
            return 100.0;
        }

        return Math.Min(100.0, (double)CurrentXp / required * 100.0);
    }
}
