using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.WorkoutExecution;

public class WorkoutSet : Entity
{
    public Guid WorkoutExerciseId { get; private set; }
    public WorkoutExercise WorkoutExercise { get; private set; } = null!;

    public Guid? SetPrescriptionId { get; private set; }
    public SetPrescription? SetPrescription { get; private set; }

    public int SetNumber { get; private set; }

    public int? TargetRepsMin { get; private set; }
    public int? TargetRepsMax { get; private set; }
    public decimal? TargetLoadValue { get; private set; }
    public PrescriptionLoadUnit TargetLoadUnit { get; private set; } = PrescriptionLoadUnit.Kilograms;
    public TimeSpan? TargetRestSeconds { get; private set; }
    public SetTechnique? TechniqueApplied { get; private set; }

    public bool IsAdditionalSet { get; private set; }

    public int? ActualReps { get; private set; }
    public decimal? ActualLoadValue { get; private set; }
    public PrescriptionLoadUnit ActualLoadUnit { get; private set; } = PrescriptionLoadUnit.Kilograms;
    public TimeSpan? ActualDuration { get; private set; }
    public decimal? DistanceKm { get; private set; }
    public decimal? SpeedKmh { get; private set; }
    public decimal? InclinePercent { get; private set; }
    public int? Calories { get; private set; }

    public bool Completed { get; private set; }
    public SetRating? DifficultyRating { get; private set; }
    public string? Notes { get; private set; }

    public decimal? VolumeKg =>
        ActualLoadValue.HasValue && ActualReps.HasValue
            ? ActualLoadValue.Value * ActualReps.Value
            : null;

    private WorkoutSet() { }

    public WorkoutSet(
        Guid workoutExerciseId,
        Guid? setPrescriptionId,
        int setNumber,
        int? targetRepsMin,
        int? targetRepsMax,
        decimal? targetLoadValue,
        PrescriptionLoadUnit targetLoadUnit,
        TimeSpan? targetRestSeconds,
        SetTechnique? techniqueApplied,
        bool isAdditionalSet = false)
    {
        if (workoutExerciseId == Guid.Empty) throw new ArgumentException("WorkoutExerciseId cannot be empty.", nameof(workoutExerciseId));
        if (setNumber < 1) throw new ArgumentOutOfRangeException(nameof(setNumber));

        WorkoutExerciseId = workoutExerciseId;
        SetPrescriptionId = setPrescriptionId;
        SetNumber = setNumber;
        TargetRepsMin = targetRepsMin;
        TargetRepsMax = targetRepsMax;
        TargetLoadValue = targetLoadValue;
        TargetLoadUnit = targetLoadUnit;
        TargetRestSeconds = targetRestSeconds;
        TechniqueApplied = techniqueApplied;
        IsAdditionalSet = isAdditionalSet;
        Completed = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void LogExecution(
        int? actualReps,
        decimal? actualLoadValue,
        PrescriptionLoadUnit? actualLoadUnit,
        TimeSpan? actualDuration,
        decimal? distanceKm,
        decimal? speedKmh,
        decimal? inclinePercent,
        int? calories,
        bool completed,
        SetRating? difficultyRating,
        string? notes)
    {
        if (actualReps < 0) throw new ArgumentOutOfRangeException(nameof(actualReps));
        if (actualLoadValue < 0) throw new ArgumentOutOfRangeException(nameof(actualLoadValue));
        if (distanceKm < 0) throw new ArgumentOutOfRangeException(nameof(distanceKm));
        if (speedKmh < 0) throw new ArgumentOutOfRangeException(nameof(speedKmh));
        if (inclinePercent < -100 || inclinePercent > 100) throw new ArgumentOutOfRangeException(nameof(inclinePercent));
        if (calories < 0) throw new ArgumentOutOfRangeException(nameof(calories));
        if (notes != null && notes.Length > 500) throw new ArgumentException("Notes too long (> 500).", nameof(notes));

        ActualReps = actualReps;
        ActualLoadValue = actualLoadValue;
        if (actualLoadUnit.HasValue) ActualLoadUnit = actualLoadUnit.Value;
        ActualDuration = actualDuration;
        DistanceKm = distanceKm;
        SpeedKmh = speedKmh;
        InclinePercent = inclinePercent;
        Calories = calories;
        Completed = completed;
        DifficultyRating = difficultyRating;
        Notes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
