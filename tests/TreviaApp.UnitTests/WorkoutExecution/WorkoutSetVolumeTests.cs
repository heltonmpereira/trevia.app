using TreviaApp.Domain.WorkoutExecution;

namespace TreviaApp.UnitTests.WorkoutExecution;

public class WorkoutSetVolumeTests
{
    private static WorkoutSet CreateSet(int setNumber = 1, int? targetRepsMin = 8)
    {
        return new WorkoutSet(
            workoutExerciseId: Guid.NewGuid(),
            setPrescriptionId: null,
            setNumber: setNumber,
            targetRepsMin: targetRepsMin,
            targetRepsMax: 12,
            targetLoadValue: 20,
            targetLoadUnit: PrescriptionLoadUnit.Kilograms,
            targetRestSeconds: TimeSpan.FromSeconds(90),
            techniqueApplied: SetTechnique.Standard);
    }

    [Fact]
    public void VolumeKg_BothLoadAndRepsSet_ReturnsProduct()
    {
        var set = CreateSet();
        set.LogExecution(
            actualReps: 10,
            actualLoadValue: 30,
            actualLoadUnit: PrescriptionLoadUnit.Kilograms,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: true,
            difficultyRating: SetRating.Moderate,
            notes: null);

        set.VolumeKg.Should().Be(30 * 10);
    }

    [Fact]
    public void VolumeKg_OnlyRepsNoLoad_ReturnsNull()
    {
        var set = CreateSet();
        set.LogExecution(
            actualReps: 12,
            actualLoadValue: null,
            actualLoadUnit: null,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: true,
            difficultyRating: SetRating.Easy,
            notes: null);

        set.VolumeKg.Should().BeNull();
    }

    [Fact]
    public void VolumeKg_OnlyLoadNoReps_ReturnsNull()
    {
        var set = CreateSet();
        set.LogExecution(
            actualReps: null,
            actualLoadValue: 50,
            actualLoadUnit: PrescriptionLoadUnit.Kilograms,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: false,
            difficultyRating: null,
            notes: null);

        set.VolumeKg.Should().BeNull();
    }

    [Fact]
    public void VolumeKg_NeitherSet_ReturnsNull()
    {
        var set = CreateSet();
        set.VolumeKg.Should().BeNull();
    }

    [Fact]
    public void VolumeKg_ZeroLoad_ReturnsZero()
    {
        var set = CreateSet();
        set.LogExecution(
            actualReps: 10,
            actualLoadValue: 0,
            actualLoadUnit: PrescriptionLoadUnit.Kilograms,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: true,
            difficultyRating: SetRating.Easy,
            notes: null);

        set.VolumeKg.Should().Be(0);
    }

    [Fact]
    public void VolumeKg_DecimalLoad_CorrectlyCalculated()
    {
        var set = CreateSet();
        set.LogExecution(
            actualReps: 8,
            actualLoadValue: 22.5m,
            actualLoadUnit: PrescriptionLoadUnit.Kilograms,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: true,
            difficultyRating: SetRating.Hard,
            notes: null);

        set.VolumeKg.Should().Be(22.5m * 8);
    }

    [Fact]
    public void LogExecution_NegativeReps_Throws()
    {
        var set = CreateSet();
        var act = () => set.LogExecution(
            actualReps: -1,
            actualLoadValue: 20,
            actualLoadUnit: null,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: false,
            difficultyRating: null,
            notes: null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void LogExecution_NegativeLoad_Throws()
    {
        var set = CreateSet();
        var act = () => set.LogExecution(
            actualReps: 10,
            actualLoadValue: -5,
            actualLoadUnit: null,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: false,
            difficultyRating: null,
            notes: null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void LogExecution_InvalidInclineBelowMinus100_Throws()
    {
        var set = CreateSet();
        var act = () => set.LogExecution(
            actualReps: 10,
            actualLoadValue: 20,
            actualLoadUnit: null,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: -101,
            calories: null,
            completed: true,
            difficultyRating: null,
            notes: null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void LogExecution_InvalidInclineAbove100_Throws()
    {
        var set = CreateSet();
        var act = () => set.LogExecution(
            actualReps: 10,
            actualLoadValue: 20,
            actualLoadUnit: null,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: 101,
            calories: null,
            completed: true,
            difficultyRating: null,
            notes: null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void LogExecution_NotesTooLong_Throws()
    {
        var set = CreateSet();
        string longNotes = new string('A', 501);
        var act = () => set.LogExecution(
            actualReps: 10,
            actualLoadValue: 20,
            actualLoadUnit: null,
            actualDuration: null,
            distanceKm: null,
            speedKmh: null,
            inclinePercent: null,
            calories: null,
            completed: true,
            difficultyRating: null,
            notes: longNotes);

        act.Should().Throw<ArgumentException>().WithMessage("*Notes too long*");
    }

    [Fact]
    public void Constructor_EmptyWorkoutExerciseId_Throws()
    {
        var act = () => new WorkoutSet(
            workoutExerciseId: Guid.Empty,
            setPrescriptionId: null,
            setNumber: 1,
            targetRepsMin: 8,
            targetRepsMax: 12,
            targetLoadValue: 20,
            targetLoadUnit: PrescriptionLoadUnit.Kilograms,
            targetRestSeconds: null,
            techniqueApplied: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ZeroSetNumber_Throws()
    {
        var act = () => new WorkoutSet(
            workoutExerciseId: Guid.NewGuid(),
            setPrescriptionId: null,
            setNumber: 0,
            targetRepsMin: 8,
            targetRepsMax: 12,
            targetLoadValue: 20,
            targetLoadUnit: PrescriptionLoadUnit.Kilograms,
            targetRestSeconds: null,
            techniqueApplied: null);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
