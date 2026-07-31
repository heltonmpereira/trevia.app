using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

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
