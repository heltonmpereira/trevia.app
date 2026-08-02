using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.WorkoutExecution.Requests;

/// <summary>
/// Request payload to start a workout session.
/// </summary>
public sealed record StartWorkoutSessionRequest
{
    /// <summary>
    /// Initializes a new instance of <see cref="StartWorkoutSessionRequest"/>.
    /// </summary>
    public StartWorkoutSessionRequest() { }

    /// <summary>
    /// Initializes a new instance of <see cref="StartWorkoutSessionRequest"/>.
    /// </summary>
    public StartWorkoutSessionRequest(Guid trainingSessionId, int weekNumberInPlan = 1)
    {
        TrainingSessionId = trainingSessionId;
        WeekNumberInPlan = weekNumberInPlan;
    }

    /// <summary>
    /// Gets the training session identifier to start.
    /// </summary>
    public Guid TrainingSessionId { get; init; }

    /// <summary>
    /// Gets the week number within the plan.
    /// </summary>
    public int WeekNumberInPlan { get; init; } = 1;
}

/// <summary>
/// Request payload to finish a workout session.
/// </summary>
public sealed record FinishWorkoutSessionRequest
{
    /// <summary>
    /// Initializes a new instance of <see cref="FinishWorkoutSessionRequest"/>.
    /// </summary>
    public FinishWorkoutSessionRequest() { }

    /// <summary>
    /// Initializes a new instance of <see cref="FinishWorkoutSessionRequest"/>.
    /// </summary>
    public FinishWorkoutSessionRequest(WorkoutRating? overallRating = null, string? generalNotes = null, int? caloriesBurned = null)
    {
        OverallRating = overallRating;
        GeneralNotes = generalNotes;
        CaloriesBurned = caloriesBurned;
    }

    /// <summary>
    /// Gets the overall perceived effort rating, when available.
    /// </summary>
    public WorkoutRating? OverallRating { get; init; }

    /// <summary>
    /// Gets the general notes captured at the end of the workout, when available.
    /// </summary>
    public string? GeneralNotes { get; init; }

    /// <summary>
    /// Gets the calories burned, when available.
    /// </summary>
    public int? CaloriesBurned { get; init; }
}

/// <summary>
/// Request payload to skip a workout exercise.
/// </summary>
public sealed record SkipWorkoutExerciseRequest
{
    /// <summary>
    /// Initializes a new instance of <see cref="SkipWorkoutExerciseRequest"/>.
    /// </summary>
    public SkipWorkoutExerciseRequest() { }

    /// <summary>
    /// Initializes a new instance of <see cref="SkipWorkoutExerciseRequest"/>.
    /// </summary>
    public SkipWorkoutExerciseRequest(string? skipReason = null)
    {
        SkipReason = skipReason;
    }

    /// <summary>
    /// Gets the reason for skipping the exercise, when available.
    /// </summary>
    public string? SkipReason { get; init; }
}

/// <summary>
/// Request payload to add an extra set during execution.
/// </summary>
public sealed record AddExtraSetRequest
{
    /// <summary>
    /// Initializes a new instance of <see cref="AddExtraSetRequest"/>.
    /// </summary>
    public AddExtraSetRequest() { }

    /// <summary>
    /// Initializes a new instance of <see cref="AddExtraSetRequest"/>.
    /// </summary>
    public AddExtraSetRequest(int? suggestedSetNumber = null)
    {
        SuggestedSetNumber = suggestedSetNumber;
    }

    /// <summary>
    /// Gets the suggested set number to use, when available.
    /// </summary>
    public int? SuggestedSetNumber { get; init; }
}

/// <summary>
/// Request payload to log an executed workout set.
/// </summary>
public sealed record LogWorkoutSetRequest
{
    /// <summary>
    /// Initializes a new instance of <see cref="LogWorkoutSetRequest"/>.
    /// </summary>
    public LogWorkoutSetRequest() { }

    /// <summary>
    /// Initializes a new instance of <see cref="LogWorkoutSetRequest"/>.
    /// </summary>
    public LogWorkoutSetRequest(
        int? actualReps = null,
        decimal? actualLoadValue = null,
        PrescriptionLoadUnit? actualLoadUnit = null,
        long? actualDurationSeconds = null,
        decimal? distanceKm = null,
        decimal? speedKmh = null,
        decimal? inclinePercent = null,
        int? calories = null,
        bool completed = true,
        SetRating? difficultyRating = null,
        string? notes = null)
    {
        ActualReps = actualReps;
        ActualLoadValue = actualLoadValue;
        ActualLoadUnit = actualLoadUnit;
        ActualDurationSeconds = actualDurationSeconds;
        DistanceKm = distanceKm;
        SpeedKmh = speedKmh;
        InclinePercent = inclinePercent;
        Calories = calories;
        Completed = completed;
        DifficultyRating = difficultyRating;
        Notes = notes;
    }

    /// <summary>
    /// Gets the actual repetitions performed, when available.
    /// </summary>
    public int? ActualReps { get; init; }

    /// <summary>
    /// Gets the actual load value used, when available.
    /// </summary>
    public decimal? ActualLoadValue { get; init; }

    /// <summary>
    /// Gets the actual load unit used, when available.
    /// </summary>
    public PrescriptionLoadUnit? ActualLoadUnit { get; init; }

    /// <summary>
    /// Gets the actual duration in seconds for time-based efforts, when available.
    /// </summary>
    public long? ActualDurationSeconds { get; init; }

    /// <summary>
    /// Gets the distance performed in kilometers, when available.
    /// </summary>
    public decimal? DistanceKm { get; init; }

    /// <summary>
    /// Gets the speed performed in kilometers per hour, when available.
    /// </summary>
    public decimal? SpeedKmh { get; init; }

    /// <summary>
    /// Gets the incline percentage, when available.
    /// </summary>
    public decimal? InclinePercent { get; init; }

    /// <summary>
    /// Gets the calories informed for the set, when available.
    /// </summary>
    public int? Calories { get; init; }

    /// <summary>
    /// Gets a value indicating whether the set was completed.
    /// </summary>
    public bool Completed { get; init; } = true;

    /// <summary>
    /// Gets the perceived difficulty rating, when available.
    /// </summary>
    public SetRating? DifficultyRating { get; init; }

    /// <summary>
    /// Gets optional notes captured for the set, when available.
    /// </summary>
    public string? Notes { get; init; }
}
