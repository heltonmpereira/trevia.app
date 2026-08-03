using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Constants;

namespace TreviaApp.Domain.Gamification;

public class LevelCurve : ValueObject
{
    public static long XpRequiredForLevel(int level)
    {
        if (level <= 0)
        {
            return 0;
        }

        if (level >= GamificationConstants.MaxLevel)
        {
            return long.MaxValue;
        }

        double xp = 100.0 * Math.Pow(level, 1.8) + 50.0 * level;
        return (long)Math.Round(xp);
    }

    public static long TotalXpForLevel(int level)
    {
        long total = 0;
        for (int i = 1; i < level; i++)
        {
            total += XpRequiredForLevel(i);
        }

        return total;
    }

    public static int CalculateLevelFromTotalXp(long totalXp)
    {
        int level = 1;
        long remaining = totalXp;

        while (level < GamificationConstants.MaxLevel)
        {
            long required = XpRequiredForLevel(level);
            if (remaining < required)
            {
                break;
            }

            remaining -= required;
            level++;
        }

        return level;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return GamificationConstants.MaxLevel;
    }
}
