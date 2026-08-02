using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

/// <summary>
/// Request payload for AddEquipmentToExerciseRequest.
/// </summary>
/// <param name="Equipment">Equipment value.</param>
/// <param name="Required">Required value.</param>
public sealed record AddEquipmentToExerciseRequest(Equipment Equipment, bool Required = true);
