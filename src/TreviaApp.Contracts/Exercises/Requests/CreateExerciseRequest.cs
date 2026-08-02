using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

/// <summary>
/// Request payload for CreateExerciseRequest.
/// </summary>
/// <param name="Name">Name value.</param>
/// <param name="Environment">Environment value.</param>
/// <param name="Modality">Modality value.</param>
/// <param name="DifficultyLevel">Difficulty Level value.</param>
/// <param name="MeasurementType">Measurement Type value.</param>
/// <param name="Instructions">Instructions value.</param>
/// <param name="ShortDescription">Short Description value.</param>
/// <param name="Tips">Tips value.</param>
/// <param name="Tags">Tags value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="Muscles">Muscles value.</param>
/// <param name="Equipments">Equipments value.</param>
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

/// <summary>
/// Request payload for MuscleMappingRequest.
/// </summary>
/// <param name="Muscle">Muscle value.</param>
/// <param name="Role">Role value.</param>
/// <param name="ActivationPercent">Activation Percent value.</param>
public sealed record MuscleMappingRequest(Muscle Muscle, MuscleRole Role = MuscleRole.Primary, decimal? ActivationPercent = null);

/// <summary>
/// Request payload for EquipmentMappingRequest.
/// </summary>
/// <param name="Equipment">Equipment value.</param>
/// <param name="Required">Required value.</param>
public sealed record EquipmentMappingRequest(Equipment Equipment, bool Required = true);
