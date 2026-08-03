using TreviaApp.Domain.Gamification;

namespace TreviaApp.UnitTests.Gamification;

public class UserLevelTests
{
    [Fact]
    public void Constructor_DefaultState_Level1ZeroXp()
    {
        var userId = Guid.NewGuid();
        var ul = new UserLevel(userId);

        ul.UserId.Should().Be(userId);
        ul.CurrentLevel.Should().Be(1);
        ul.CurrentXp.Should().Be(0);
        ul.TotalXpEarned.Should().Be(0);
    }

    [Fact]
    public void AddXp_ZeroOrNegative_NoChange()
    {
        var ul = new UserLevel(Guid.NewGuid());

        var result = ul.AddXp(0);
        result.LeveledUp.Should().BeFalse();
        result.NewLevel.Should().Be(1);
        result.BonusPoints.Should().Be(0);
        ul.CurrentXp.Should().Be(0);
        ul.TotalXpEarned.Should().Be(0);

        var neg = ul.AddXp(-100);
        neg.LeveledUp.Should().BeFalse();
        ul.CurrentXp.Should().Be(0);
    }

    [Fact]
    public void AddXp_SmallAmount_NoLevelUp()
    {
        var ul = new UserLevel(Guid.NewGuid());
        long requiredL1 = LevelCurve.XpRequiredForLevel(1);
        long smallXp = requiredL1 / 2;

        var result = ul.AddXp(smallXp);

        result.LeveledUp.Should().BeFalse();
        result.NewLevel.Should().Be(1);
        result.BonusPoints.Should().Be(0);
        ul.CurrentLevel.Should().Be(1);
        ul.CurrentXp.Should().Be(smallXp);
        ul.TotalXpEarned.Should().Be(smallXp);
    }

    [Fact]
    public void AddXp_ExactlyLevelRequired_LevelsUpWithBonus()
    {
        var ul = new UserLevel(Guid.NewGuid());
        long requiredL1 = LevelCurve.XpRequiredForLevel(1);

        var result = ul.AddXp(requiredL1);

        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(2);
        result.BonusPoints.Should().BeGreaterThan(0);
        ul.CurrentLevel.Should().Be(2);
        ul.TotalXpEarned.Should().Be(requiredL1);
    }

    [Fact]
    public void AddXp_MultiLevelUp_LevelsUpMultipleTimes()
    {
        var ul = new UserLevel(Guid.NewGuid());
        long bigXp = LevelCurve.TotalXpForLevel(5);

        var result = ul.AddXp(bigXp);

        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(5);
        ul.CurrentLevel.Should().Be(5);
        result.BonusPoints.Should().BePositive();
    }

    [Fact]
    public void XpToNextLevel_Level1_ReturnsCorrectAmount()
    {
        var ul = new UserLevel(Guid.NewGuid());
        var toNext = ul.XpToNextLevel();

        toNext.Should().Be(LevelCurve.XpRequiredForLevel(1));
    }

    [Fact]
    public void XpToNextLevel_AfterPartiallyFilled_RemainingCorrect()
    {
        var ul = new UserLevel(Guid.NewGuid());
        long requiredL1 = LevelCurve.XpRequiredForLevel(1);
        ul.AddXp(requiredL1 / 4);

        var remaining = ul.XpToNextLevel();
        remaining.Should().Be(requiredL1 - (requiredL1 / 4));
    }

    [Fact]
    public void XpToNextLevel_AfterLevelUp_CalculatedFromNewLevel()
    {
        var ul = new UserLevel(Guid.NewGuid());
        ul.AddXp(LevelCurve.XpRequiredForLevel(1));
        ul.CurrentLevel.Should().Be(2);

        var toNext = ul.XpToNextLevel();
        toNext.Should().Be(LevelCurve.XpRequiredForLevel(2) - ul.CurrentXp);
    }

    [Fact]
    public void ProgressPercentage_Level1NoXp_ZeroPercent()
    {
        var ul = new UserLevel(Guid.NewGuid());
        ul.ProgressPercentageToNextLevel().Should().BeApproximately(0.0, precision: 0.01);
    }

    [Fact]
    public void ProgressPercentage_HalfWay_50Percent()
    {
        var ul = new UserLevel(Guid.NewGuid());
        long requiredL1 = LevelCurve.XpRequiredForLevel(1);
        ul.AddXp(requiredL1 / 2);

        var pct = ul.ProgressPercentageToNextLevel();
        pct.Should().BeApproximately(50.0, precision: 1.0);
    }

    [Fact]
    public void ProgressPercentage_DoesNotExceed100()
    {
        var ul = new UserLevel(Guid.NewGuid());
        long requiredL1 = LevelCurve.XpRequiredForLevel(1);
        ul.AddXp(requiredL1 * 10);

        ul.ProgressPercentageToNextLevel().Should().BeLessThanOrEqualTo(100.0);
    }

    [Fact]
    public void DailyWorkoutCapConstants_AreDefined()
    {
        GamificationConstants.DailyWorkoutAwardCap.Should().BeGreaterOrEqualTo(1);
        GamificationConstants.DailySetPointsCap.Should().BeGreaterOrEqualTo(10);
        GamificationConstants.WorkoutCompletedPoints.Should().BeGreaterThan(0);
        GamificationConstants.SetCompletedPoints.Should().BeGreaterThan(0);
    }

    [Fact]
    public void WorkoutCompletedPoints_ExceedsDailyCap_DoesNotLevelDown()
    {
        var ul = new UserLevel(Guid.NewGuid());
        var firstAdd = ul.AddXp(GamificationConstants.WorkoutCompletedPoints * GamificationConstants.XpPerPoint);
        var secondAdd = ul.AddXp(GamificationConstants.WorkoutCompletedPoints * GamificationConstants.XpPerPoint);

        ul.CurrentLevel.Should().BeGreaterOrEqualTo(1);
        ul.TotalXpEarned.Should().BePositive();
    }
}
