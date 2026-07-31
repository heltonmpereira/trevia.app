using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

public sealed record ExerciseSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    TrainingEnvironment Environment,
    ExerciseModality Modality,
    DifficultyLevel DifficultyLevel,
    MeasurementType MeasurementType,
    Visibility Visibility,
    ExerciseStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? PrimaryMediaUrl,
    int PrimaryMusclesCount,
    int EquipmentsCount);
