namespace TreviaApp.Application.Exercises.Commands.CreateExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;

public sealed record CreateExerciseCommand(
    string Name,
    Shared.Enums.TrainingEnvironment Environment,
    Shared.Enums.ExerciseModality Modality,
    Shared.Enums.DifficultyLevel DifficultyLevel,
    Shared.Enums.MeasurementType MeasurementType,
    string Instructions,
    string? ShortDescription = null,
    string? Tips = null,
    string? Tags = null,
    Shared.Enums.Visibility Visibility = Shared.Enums.Visibility.Private,
    IEnumerable<MuscleMappingRequest>? Muscles = null,
    IEnumerable<EquipmentMappingRequest>? Equipments = null)
    : ICommand<ExerciseDetailResponse>;
