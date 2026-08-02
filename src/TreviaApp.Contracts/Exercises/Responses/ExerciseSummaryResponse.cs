using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

/// <summary>
/// Response payload for ExerciseSummaryResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Name">Name value.</param>
/// <param name="Slug">Slug value.</param>
/// <param name="ShortDescription">Short Description value.</param>
/// <param name="Environment">Environment value.</param>
/// <param name="Modality">Modality value.</param>
/// <param name="DifficultyLevel">Difficulty Level value.</param>
/// <param name="MeasurementType">Measurement Type value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="Status">Status value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="UpdatedAt">Updated At value.</param>
/// <param name="PrimaryMediaUrl">Primary Media Url value.</param>
/// <param name="PrimaryMusclesCount">Primary Muscles Count value.</param>
/// <param name="EquipmentsCount">Equipments Count value.</param>
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
