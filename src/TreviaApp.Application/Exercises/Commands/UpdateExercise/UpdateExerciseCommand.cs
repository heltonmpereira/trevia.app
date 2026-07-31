namespace TreviaApp.Application.Exercises.Commands.UpdateExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

public sealed record UpdateExerciseCommand(
    Guid ExerciseId,
    string Name,
    string Instructions,
    TrainingEnvironment Environment,
    ExerciseModality Modality,
    DifficultyLevel DifficultyLevel,
    MeasurementType MeasurementType,
    Visibility Visibility,
    string? ShortDescription = null,
    string? Tips = null,
    string? Tags = null)
    : ICommand<ExerciseDetailResponse>;
