using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

/// <summary>
/// Request payload for UpdateExerciseRequest.
/// </summary>
/// <param name="Name">Name value.</param>
/// <param name="Instructions">Instructions value.</param>
/// <param name="Environment">Environment value.</param>
/// <param name="Modality">Modality value.</param>
/// <param name="DifficultyLevel">Difficulty Level value.</param>
/// <param name="MeasurementType">Measurement Type value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="ShortDescription">Short Description value.</param>
/// <param name="Tips">Tips value.</param>
/// <param name="Tags">Tags value.</param>
public sealed record UpdateExerciseRequest(
    string Name,
    string Instructions,
    TrainingEnvironment Environment,
    ExerciseModality Modality,
    DifficultyLevel DifficultyLevel,
    MeasurementType MeasurementType,
    Visibility Visibility,
    string? ShortDescription = null,
    string? Tips = null,
    string? Tags = null);
