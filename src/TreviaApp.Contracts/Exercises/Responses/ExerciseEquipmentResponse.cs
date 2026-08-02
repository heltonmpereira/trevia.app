using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

/// <summary>
/// Response payload for ExerciseEquipmentResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Equipment">Equipment value.</param>
/// <param name="Required">Required value.</param>
public sealed record ExerciseEquipmentResponse(
    Guid Id,
    Equipment Equipment,
    bool Required);
