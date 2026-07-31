using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

public sealed record ExerciseEquipmentResponse(
    Guid Id,
    Equipment Equipment,
    bool Required);
