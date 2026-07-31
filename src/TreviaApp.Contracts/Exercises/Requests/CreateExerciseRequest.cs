using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

public sealed record CreateExerciseRequest(
    string Name,
    TrainingEnvironment Environment,
    ExerciseModality Modality,
    DifficultyLevel DifficultyLevel,
    MeasurementType MeasurementType,
    string Instructions,
    string? ShortDescription = null,
    string? Tips = null,
    string? Tags = null,
    Visibility Visibility = Visibility.Private,
    IEnumerable<MuscleMappingRequest>? Muscles = null,
    IEnumerable<EquipmentMappingRequest>? Equipments = null);

public sealed record MuscleMappingRequest(Muscle Muscle, MuscleRole Role = MuscleRole.Primary, decimal? ActivationPercent = null);

public sealed record EquipmentMappingRequest(Equipment Equipment, bool Required = true);
