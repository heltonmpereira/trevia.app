using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.WorkoutExecution.Requests;

public sealed record StartWorkoutSessionRequest(
    Guid TrainingSessionId,
    int WeekNumberInPlan = 1);

public sealed record FinishWorkoutSessionRequest(
    WorkoutRating? OverallRating = null,
    string? GeneralNotes = null,
    int? CaloriesBurned = null);

public sealed record SkipWorkoutExerciseRequest(
    string? SkipReason = null);

public sealed record AddExtraSetRequest(
    int? SuggestedSetNumber = null);

public sealed record LogWorkoutSetRequest(
    int? ActualReps = null,
    decimal? ActualLoadValue = null,
    PrescriptionLoadUnit? ActualLoadUnit = null,
    long? ActualDurationSeconds = null,
    decimal? DistanceKm = null,
    decimal? SpeedKmh = null,
    decimal? InclinePercent = null,
    int? Calories = null,
    bool Completed = true,
    SetRating? DifficultyRating = null,
    string? Notes = null);
