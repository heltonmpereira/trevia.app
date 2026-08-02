using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.WorkoutExecution;

/// <summary>
/// Represents the WorkoutSet domain entity.
/// </summary>
public class WorkoutSet : Entity
{
    /// <summary>
    /// Gets Workout Exercise Id.
    /// </summary>
    public Guid WorkoutExerciseId { get; private set; }

    /// <summary>
    /// Gets Workout Exercise.
    /// </summary>
    public WorkoutExercise WorkoutExercise { get; private set; } = null!;

    /// <summary>
    /// Gets Set Prescription Id.
    /// </summary>
    public Guid? SetPrescriptionId { get; private set; }

    /// <summary>
    /// Gets Set Prescription.
    /// </summary>
    public SetPrescription? SetPrescription { get; private set; }

    /// <summary>
    /// Gets Set Number.
    /// </summary>
    public int SetNumber { get; private set; }

    /// <summary>
    /// Gets Target Reps Min.
    /// </summary>
    public int? TargetRepsMin { get; private set; }

    /// <summary>
    /// Gets Target Reps Max.
    /// </summary>
    public int? TargetRepsMax { get; private set; }

    /// <summary>
    /// Gets Target Load Value.
    /// </summary>
    public decimal? TargetLoadValue { get; private set; }

    /// <summary>
    /// Gets Target Load Unit.
    /// </summary>
    public PrescriptionLoadUnit TargetLoadUnit { get; private set; } = PrescriptionLoadUnit.Kilograms;

    /// <summary>
    /// Gets Target Rest Seconds.
    /// </summary>
    public TimeSpan? TargetRestSeconds { get; private set; }

    /// <summary>
    /// Gets Technique Applied.
    /// </summary>
    public SetTechnique? TechniqueApplied { get; private set; }

    /// <summary>
    /// Gets Is Additional Set.
    /// </summary>
    public bool IsAdditionalSet { get; private set; }

    /// <summary>
    /// Gets Actual Reps.
    /// </summary>
    public int? ActualReps { get; private set; }

    /// <summary>
    /// Gets Actual Load Value.
    /// </summary>
    public decimal? ActualLoadValue { get; private set; }

    /// <summary>
    /// Gets Actual Load Unit.
    /// </summary>
    public PrescriptionLoadUnit ActualLoadUnit { get; private set; } = PrescriptionLoadUnit.Kilograms;

    /// <summary>
    /// Gets Actual Duration.
    /// </summary>
    public TimeSpan? ActualDuration { get; private set; }

    /// <summary>
    /// Gets Distance Km.
    /// </summary>
    public decimal? DistanceKm { get; private set; }

    /// <summary>
    /// Gets Speed Kmh.
    /// </summary>
    public decimal? SpeedKmh { get; private set; }

    /// <summary>
    /// Gets Incline Percent.
    /// </summary>
    public decimal? InclinePercent { get; private set; }

    /// <summary>
    /// Gets Calories.
    /// </summary>
    public int? Calories { get; private set; }

    /// <summary>
    /// Gets Completed.
    /// </summary>
    public bool Completed { get; private set; }

    /// <summary>
    /// Gets Difficulty Rating.
    /// </summary>
    public SetRating? DifficultyRating { get; private set; }

    /// <summary>
    /// Gets Notes.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets Volume Kg.
    /// </summary>
    public decimal? VolumeKg =>
        ActualLoadValue.HasValue && ActualReps.HasValue
            ? ActualLoadValue.Value * ActualReps.Value
            : null;

    private WorkoutSet() { }

    /// <summary>
    /// Initializes a new instance of the WorkoutSet class.
    /// </summary>
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

    /// <summary>
    /// Executes Log Execution.
    /// </summary>
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
