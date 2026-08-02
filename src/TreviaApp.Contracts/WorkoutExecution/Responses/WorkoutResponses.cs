using TreviaApp.Contracts.Common;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.WorkoutExecution.Responses;

public sealed record WorkoutSessionSummaryResponse(
    Guid Id,
    Guid? TrainingPlanId,
    string? TrainingPlanName,
    Guid? TrainingSessionId,
    string Name,
    WorkoutStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    long? TotalDurationElapsedSeconds,
    long? ActiveTimeSeconds,
    WorkoutRating? OverallRating,
    int WeekNumberInPlan,
    int ExercisesCount,
    int CompletedSetsCount,
    decimal? TotalVolumeKg);

public sealed record WorkoutSessionDetailResponse(
    Guid Id,
    Guid StudentId,
    string StudentDisplayName,
    string? StudentPhotoFileId,
    Guid? TrainingPlanId,
    string? TrainingPlanName,
    Guid? TrainingSessionId,
    string Name,
    WorkoutStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    long? TotalDurationElapsedSeconds,
    long? ActiveTimeSeconds,
    int? CaloriesBurned,
    WorkoutRating? OverallRating,
    string? GeneralNotes,
    int WeekNumberInPlan,
    IEnumerable<WorkoutExerciseResponse> Exercises,
    IEnumerable<WorkoutPauseResponse> Pauses);

public sealed record WorkoutExerciseResponse(
    Guid Id,
    Guid? SessionExerciseId,
    Guid ExerciseId,
    string ExerciseName,
    int Order,
    bool IsSkipped,
    string? SkipReason,
    string? Notes,
    IEnumerable<WorkoutSetResponse> Sets);

public sealed record WorkoutSetResponse(
    Guid Id,
    Guid? SetPrescriptionId,
    int SetNumber,
    int? TargetRepsMin,
    int? TargetRepsMax,
    decimal? TargetLoadValue,
    PrescriptionLoadUnit TargetLoadUnit,
    int? ActualReps,
    decimal? ActualLoadValue,
    PrescriptionLoadUnit ActualLoadUnit,
    long? ActualDurationSeconds,
    decimal? DistanceKm,
    decimal? SpeedKmh,
    decimal? InclinePercent,
    int? Calories,
    bool Completed,
    SetRating? DifficultyRating,
    string? Notes,
    bool IsAdditionalSet,
    decimal? VolumeKg);

public sealed record WorkoutPauseResponse(
    Guid Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    long? DurationSeconds);

public class WorkoutSessionsPagedResponse : PaginatedResponse<WorkoutSessionSummaryResponse>
{
}
