using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;

namespace TreviaApp.Domain.Gamification;

public class UserStreak : Entity
{
    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public int DailyCurrent { get; private set; } = 0;

    public int DailyLongest { get; private set; } = 0;

    public DateOnly? DailyLastActiveAt { get; private set; }

    public int WeeklyCurrent { get; private set; } = 0;

    public int WeeklyLongest { get; private set; } = 0;

    public DateOnly? WeekStartDate { get; private set; }

    private UserStreak()
    {
    }

    public UserStreak(Guid userId)
    {
        UserId = userId;
    }

    public void UpdateDaily(DateOnly workoutDate)
    {
        if (!DailyLastActiveAt.HasValue)
        {
            DailyCurrent = 1;
            DailyLastActiveAt = workoutDate;
        }
        else
        {
            int daysDiff = workoutDate.DayNumber - DailyLastActiveAt.Value.DayNumber;

            if (daysDiff == 0)
            {
            }
            else if (daysDiff == 1)
            {
                DailyCurrent++;
                DailyLastActiveAt = workoutDate;
            }
            else
            {
                DailyCurrent = 1;
                DailyLastActiveAt = workoutDate;
            }
        }

        if (DailyCurrent > DailyLongest)
        {
            DailyLongest = DailyCurrent;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateWeekly(DateOnly weekStart, int activeDaysThisWeek)
    {
        WeekStartDate = weekStart;
        WeeklyCurrent = activeDaysThisWeek > 0 ? activeDaysThisWeek : 0;

        if (WeeklyCurrent > WeeklyLongest)
        {
            WeeklyLongest = WeeklyCurrent;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reset()
    {
        DailyCurrent = 0;
        DailyLastActiveAt = null;
        WeeklyCurrent = 0;
        WeekStartDate = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDailyLongest(int longest)
    {
        if (longest > DailyLongest)
        {
            DailyLongest = longest;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void SetWeeklyLongest(int longest)
    {
        if (longest > WeeklyLongest)
        {
            WeeklyLongest = longest;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int AwardedStreaks()
    {
        int awarded = 0;
        if (DailyCurrent >= 7) awarded++;
        if (DailyCurrent >= 30) awarded++;
        return awarded;
    }
}
