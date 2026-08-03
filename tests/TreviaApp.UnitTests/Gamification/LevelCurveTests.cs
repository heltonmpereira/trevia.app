using TreviaApp.Domain.Gamification;

namespace TreviaApp.UnitTests.Gamification;

public class LevelCurveTests
{
    [Fact]
    public void XpRequiredForLevel_Level0_Returns0()
    {
        var result = LevelCurve.XpRequiredForLevel(0);
        result.Should().Be(0);
    }

    [Fact]
    public void XpRequiredForLevel_Level1_ReturnsExpectedValue()
    {
        var result = LevelCurve.XpRequiredForLevel(1);
        result.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(99)]
    public void XpRequiredForLevel_LevelPositive_ReturnsPositive(int level)
    {
        var result = LevelCurve.XpRequiredForLevel(level);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void XpRequiredForLevel_MaxLevel_ReturnsLongMaxValue()
    {
        var result = LevelCurve.XpRequiredForLevel(GamificationConstants.MaxLevel);
        result.Should().Be(long.MaxValue);
    }

    [Fact]
    public void XpRequiredForLevel_AboveMaxLevel_ReturnsLongMaxValue()
    {
        var result = LevelCurve.XpRequiredForLevel(GamificationConstants.MaxLevel + 10);
        result.Should().Be(long.MaxValue);
    }

    [Fact]
    public void XpRequiredForLevel_Level1_ShouldBeLessThanLevel2()
    {
        var l1 = LevelCurve.XpRequiredForLevel(1);
        var l2 = LevelCurve.XpRequiredForLevel(2);
        l1.Should().BeLessThan(l2);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(10, 11)]
    [InlineData(50, 51)]
    public void XpRequiredForLevel_ShouldBeMonotonicallyIncreasing(int lower, int higher)
    {
        var lowerXp = LevelCurve.XpRequiredForLevel(lower);
        var higherXp = LevelCurve.XpRequiredForLevel(higher);
        higherXp.Should().BeGreaterThan(lowerXp);
    }

    [Fact]
    public void TotalXpForLevel_Level1_Returns0()
    {
        var result = LevelCurve.TotalXpForLevel(1);
        result.Should().Be(0);
    }

    [Fact]
    public void TotalXpForLevel_Level2_EqualsLevel1Required()
    {
        var l1 = LevelCurve.XpRequiredForLevel(1);
        var totalL2 = LevelCurve.TotalXpForLevel(2);
        totalL2.Should().Be(l1);
    }

    [Fact]
    public void TotalXpForLevel_Level3_EqualsSumLevel1And2()
    {
        var l1 = LevelCurve.XpRequiredForLevel(1);
        var l2 = LevelCurve.XpRequiredForLevel(2);
        var totalL3 = LevelCurve.TotalXpForLevel(3);
        totalL3.Should().Be(l1 + l2);
    }

    [Fact]
    public void CalculateLevelFromTotalXp_ZeroXp_ReturnsLevel1()
    {
        var result = LevelCurve.CalculateLevelFromTotalXp(0);
        result.Should().Be(1);
    }

    [Fact]
    public void CalculateLevelFromTotalXp_ExactlyLevel1Required_ReachesLevel2()
    {
        var requiredL1 = LevelCurve.XpRequiredForLevel(1);
        var level = LevelCurve.CalculateLevelFromTotalXp(requiredL1);
        level.Should().Be(2);
    }

    [Fact]
    public void CalculateLevelFromTotalXp_LessThanLevel1Required_StaysLevel1()
    {
        var requiredL1 = LevelCurve.XpRequiredForLevel(1);
        var xp = requiredL1 - 1;
        var level = LevelCurve.CalculateLevelFromTotalXp(xp);
        level.Should().Be(1);
    }

    [Fact]
    public void CalculateLevelFromTotalXp_MultipleLevels_Correct()
    {
        var totalForL5 = LevelCurve.TotalXpForLevel(5);
        var level = LevelCurve.CalculateLevelFromTotalXp(totalForL5);
        level.Should().Be(5);
    }

    [Fact]
    public void CalculateLevelFromTotalXp_LargeAmount_DoesNotExceedMaxLevel()
    {
        var level = LevelCurve.CalculateLevelFromTotalXp(long.MaxValue);
        level.Should().BeLessThanOrEqualTo(GamificationConstants.MaxLevel);
    }

    [Fact]
    public void RoundTrip_TotalXpThenCalculateLevel_AreConsistent()
    {
        for (int targetLevel = 1; targetLevel <= 10; targetLevel++)
        {
            long total = LevelCurve.TotalXpForLevel(targetLevel);
            int calculated = LevelCurve.CalculateLevelFromTotalXp(total);
            calculated.Should().Be(targetLevel, because: $"level {targetLevel} total XP should map back exactly");
        }
    }
}
