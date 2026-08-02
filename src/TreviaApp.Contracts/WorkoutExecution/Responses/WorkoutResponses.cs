using TreviaApp.Contracts.Common;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.WorkoutExecution.Responses;

/// <summary>
/// Summary information for a workout session, suitable for lists and history screens.
/// </summary>
public sealed record WorkoutSessionSummaryResponse
{
    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutSessionSummaryResponse"/>.
    /// </summary>
    public WorkoutSessionSummaryResponse() { }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutSessionSummaryResponse"/>.
    /// </summary>
    public WorkoutSessionSummaryResponse(
        Guid id,
        Guid? trainingPlanId,
        string? trainingPlanName,
        Guid? trainingSessionId,
        string name,
        WorkoutStatus status,
        DateTimeOffset? startedAt,
        DateTimeOffset? finishedAt,
        long? totalDurationElapsedSeconds,
        long? activeTimeSeconds,
        WorkoutRating? overallRating,
        int weekNumberInPlan,
        int exercisesCount,
        int completedSetsCount,
        decimal? totalVolumeKg)
    {
        Id = id;
        TrainingPlanId = trainingPlanId;
        TrainingPlanName = trainingPlanName;
        TrainingSessionId = trainingSessionId;
        Name = name;
        Status = status;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        TotalDurationElapsedSeconds = totalDurationElapsedSeconds;
        ActiveTimeSeconds = activeTimeSeconds;
        OverallRating = overallRating;
        WeekNumberInPlan = weekNumberInPlan;
        ExercisesCount = exercisesCount;
        CompletedSetsCount = completedSetsCount;
        TotalVolumeKg = totalVolumeKg;
    }

    /// <summary>
    /// Gets the workout session identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the training plan identifier, when the workout came from a plan.
    /// </summary>
    public Guid? TrainingPlanId { get; init; }

    /// <summary>
    /// Gets the training plan name, when available.
    /// </summary>
    public string? TrainingPlanName { get; init; }

    /// <summary>
    /// Gets the training session identifier, when the workout came from a plan session.
    /// </summary>
    public Guid? TrainingSessionId { get; init; }

    /// <summary>
    /// Gets the workout name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current lifecycle status of the workout session.
    /// </summary>
    public WorkoutStatus Status { get; init; }

    /// <summary>
    /// Gets the start timestamp (UTC), when available.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    /// Gets the finish timestamp (UTC), when available.
    /// </summary>
    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>
    /// Gets the total elapsed duration in seconds (including pauses), when available.
    /// </summary>
    public long? TotalDurationElapsedSeconds { get; init; }

    /// <summary>
    /// Gets the active duration in seconds (excluding pauses), when available.
    /// </summary>
    public long? ActiveTimeSeconds { get; init; }

    /// <summary>
    /// Gets the overall perceived effort rating, when available.
    /// </summary>
    public WorkoutRating? OverallRating { get; init; }

    /// <summary>
    /// Gets the week number within the training plan.
    /// </summary>
    public int WeekNumberInPlan { get; init; }

    /// <summary>
    /// Gets the total number of exercises in the workout session.
    /// </summary>
    public int ExercisesCount { get; init; }

    /// <summary>
    /// Gets the total number of completed sets in the workout session.
    /// </summary>
    public int CompletedSetsCount { get; init; }

    /// <summary>
    /// Gets the total volume in kilograms, when available.
    /// </summary>
    public decimal? TotalVolumeKg { get; init; }
}

/// <summary>
/// Detailed information for a workout session.
/// </summary>
public sealed record WorkoutSessionDetailResponse
{
    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutSessionDetailResponse"/>.
    /// </summary>
    public WorkoutSessionDetailResponse() { }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutSessionDetailResponse"/>.
    /// </summary>
    public WorkoutSessionDetailResponse(
        Guid id,
        Guid studentId,
        string studentDisplayName,
        string? studentPhotoFileId,
        Guid? trainingPlanId,
        string? trainingPlanName,
        Guid? trainingSessionId,
        string name,
        WorkoutStatus status,
        DateTimeOffset? startedAt,
        DateTimeOffset? finishedAt,
        long? totalDurationElapsedSeconds,
        long? activeTimeSeconds,
        int? caloriesBurned,
        WorkoutRating? overallRating,
        string? generalNotes,
        int weekNumberInPlan,
        IEnumerable<WorkoutExerciseResponse> exercises,
        IEnumerable<WorkoutPauseResponse> pauses)
    {
        Id = id;
        StudentId = studentId;
        StudentDisplayName = studentDisplayName;
        StudentPhotoFileId = studentPhotoFileId;
        TrainingPlanId = trainingPlanId;
        TrainingPlanName = trainingPlanName;
        TrainingSessionId = trainingSessionId;
        Name = name;
        Status = status;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        TotalDurationElapsedSeconds = totalDurationElapsedSeconds;
        ActiveTimeSeconds = activeTimeSeconds;
        CaloriesBurned = caloriesBurned;
        OverallRating = overallRating;
        GeneralNotes = generalNotes;
        WeekNumberInPlan = weekNumberInPlan;
        Exercises = exercises;
        Pauses = pauses;
    }

    /// <summary>
    /// Gets the workout session identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the athlete identifier that owns the workout session.
    /// </summary>
    public Guid StudentId { get; init; }

    /// <summary>
    /// Gets the athlete display name.
    /// </summary>
    public string StudentDisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the athlete profile photo file identifier, when available.
    /// </summary>
    public string? StudentPhotoFileId { get; init; }

    /// <summary>
    /// Gets the training plan identifier, when the workout came from a plan.
    /// </summary>
    public Guid? TrainingPlanId { get; init; }

    /// <summary>
    /// Gets the training plan name, when available.
    /// </summary>
    public string? TrainingPlanName { get; init; }

    /// <summary>
    /// Gets the training session identifier, when the workout came from a plan session.
    /// </summary>
    public Guid? TrainingSessionId { get; init; }

    /// <summary>
    /// Gets the workout name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current lifecycle status of the workout session.
    /// </summary>
    public WorkoutStatus Status { get; init; }

    /// <summary>
    /// Gets the start timestamp (UTC), when available.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    /// Gets the finish timestamp (UTC), when available.
    /// </summary>
    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>
    /// Gets the total elapsed duration in seconds (including pauses), when available.
    /// </summary>
    public long? TotalDurationElapsedSeconds { get; init; }

    /// <summary>
    /// Gets the active duration in seconds (excluding pauses), when available.
    /// </summary>
    public long? ActiveTimeSeconds { get; init; }

    /// <summary>
    /// Gets the calories burned, when available.
    /// </summary>
    public int? CaloriesBurned { get; init; }

    /// <summary>
    /// Gets the overall perceived effort rating, when available.
    /// </summary>
    public WorkoutRating? OverallRating { get; init; }

    /// <summary>
    /// Gets the general notes captured at the end of the workout, when available.
    /// </summary>
    public string? GeneralNotes { get; init; }

    /// <summary>
    /// Gets the week number within the training plan.
    /// </summary>
    public int WeekNumberInPlan { get; init; }

    /// <summary>
    /// Gets the exercises executed during the workout session.
    /// </summary>
    public IEnumerable<WorkoutExerciseResponse> Exercises { get; init; } = Array.Empty<WorkoutExerciseResponse>();

    /// <summary>
    /// Gets the pauses registered during the workout session.
    /// </summary>
    public IEnumerable<WorkoutPauseResponse> Pauses { get; init; } = Array.Empty<WorkoutPauseResponse>();
}

/// <summary>
/// Detailed information for an exercise within a workout session.
/// </summary>
public sealed record WorkoutExerciseResponse
{
    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutExerciseResponse"/>.
    /// </summary>
    public WorkoutExerciseResponse() { }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutExerciseResponse"/>.
    /// </summary>
    public WorkoutExerciseResponse(
        Guid id,
        Guid? sessionExerciseId,
        Guid exerciseId,
        string exerciseName,
        int order,
        bool isSkipped,
        string? skipReason,
        string? notes,
        IEnumerable<WorkoutSetResponse> sets)
    {
        Id = id;
        SessionExerciseId = sessionExerciseId;
        ExerciseId = exerciseId;
        ExerciseName = exerciseName;
        Order = order;
        IsSkipped = isSkipped;
        SkipReason = skipReason;
        Notes = notes;
        Sets = sets;
    }

    /// <summary>
    /// Gets the workout exercise identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the training plan session exercise identifier, when the exercise came from a plan.
    /// </summary>
    public Guid? SessionExerciseId { get; init; }

    /// <summary>
    /// Gets the exercise identifier.
    /// </summary>
    public Guid ExerciseId { get; init; }

    /// <summary>
    /// Gets the exercise name.
    /// </summary>
    public string ExerciseName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display order within the workout session.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets a value indicating whether the exercise was skipped.
    /// </summary>
    public bool IsSkipped { get; init; }

    /// <summary>
    /// Gets the skip reason provided by the athlete, when available.
    /// </summary>
    public string? SkipReason { get; init; }

    /// <summary>
    /// Gets the notes associated with the exercise, when available.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the sets executed for the exercise.
    /// </summary>
    public IEnumerable<WorkoutSetResponse> Sets { get; init; } = Array.Empty<WorkoutSetResponse>();
}

/// <summary>
/// Detailed information for a set within a workout exercise.
/// </summary>
public sealed record WorkoutSetResponse
{
    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutSetResponse"/>.
    /// </summary>
    public WorkoutSetResponse() { }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutSetResponse"/>.
    /// </summary>
    public WorkoutSetResponse(
        Guid id,
        Guid? setPrescriptionId,
        int setNumber,
        int? targetRepsMin,
        int? targetRepsMax,
        decimal? targetLoadValue,
        PrescriptionLoadUnit targetLoadUnit,
        int? actualReps,
        decimal? actualLoadValue,
        PrescriptionLoadUnit actualLoadUnit,
        long? actualDurationSeconds,
        decimal? distanceKm,
        decimal? speedKmh,
        decimal? inclinePercent,
        int? calories,
        bool completed,
        SetRating? difficultyRating,
        string? notes,
        bool isAdditionalSet,
        decimal? volumeKg)
    {
        Id = id;
        SetPrescriptionId = setPrescriptionId;
        SetNumber = setNumber;
        TargetRepsMin = targetRepsMin;
        TargetRepsMax = targetRepsMax;
        TargetLoadValue = targetLoadValue;
        TargetLoadUnit = targetLoadUnit;
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
        IsAdditionalSet = isAdditionalSet;
        VolumeKg = volumeKg;
    }

    /// <summary>
    /// Gets the workout set identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the training plan set prescription identifier, when the set came from a plan.
    /// </summary>
    public Guid? SetPrescriptionId { get; init; }

    /// <summary>
    /// Gets the set ordering number within the exercise.
    /// </summary>
    public int SetNumber { get; init; }

    /// <summary>
    /// Gets the minimum target repetitions, when available.
    /// </summary>
    public int? TargetRepsMin { get; init; }

    /// <summary>
    /// Gets the maximum target repetitions, when available.
    /// </summary>
    public int? TargetRepsMax { get; init; }

    /// <summary>
    /// Gets the target load value, when available.
    /// </summary>
    public decimal? TargetLoadValue { get; init; }

    /// <summary>
    /// Gets the target load unit.
    /// </summary>
    public PrescriptionLoadUnit TargetLoadUnit { get; init; }

    /// <summary>
    /// Gets the actual repetitions performed, when available.
    /// </summary>
    public int? ActualReps { get; init; }

    /// <summary>
    /// Gets the actual load value used, when available.
    /// </summary>
    public decimal? ActualLoadValue { get; init; }

    /// <summary>
    /// Gets the actual load unit used.
    /// </summary>
    public PrescriptionLoadUnit ActualLoadUnit { get; init; }

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
    public bool Completed { get; init; }

    /// <summary>
    /// Gets the perceived difficulty rating, when available.
    /// </summary>
    public SetRating? DifficultyRating { get; init; }

    /// <summary>
    /// Gets the notes captured for the set, when available.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets a value indicating whether the set was created manually during execution.
    /// </summary>
    public bool IsAdditionalSet { get; init; }

    /// <summary>
    /// Gets the calculated volume in kilograms, when available.
    /// </summary>
    public decimal? VolumeKg { get; init; }
}

/// <summary>
/// Represents a pause interval within a workout session.
/// </summary>
public sealed record WorkoutPauseResponse
{
    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutPauseResponse"/>.
    /// </summary>
    public WorkoutPauseResponse() { }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkoutPauseResponse"/>.
    /// </summary>
    public WorkoutPauseResponse(Guid id, DateTimeOffset startedAt, DateTimeOffset? endedAt, long? durationSeconds)
    {
        Id = id;
        StartedAt = startedAt;
        EndedAt = endedAt;
        DurationSeconds = durationSeconds;
    }

    /// <summary>
    /// Gets the workout pause identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the pause start timestamp (UTC).
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Gets the pause end timestamp (UTC), when available.
    /// </summary>
    public DateTimeOffset? EndedAt { get; init; }

    /// <summary>
    /// Gets the pause duration in seconds, when available.
    /// </summary>
    public long? DurationSeconds { get; init; }
}

/// <summary>
/// Paged response containing workout session summaries.
/// </summary>
public class WorkoutSessionsPagedResponse : PaginatedResponse<WorkoutSessionSummaryResponse>
{
}
