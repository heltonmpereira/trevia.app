using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

public sealed record AddEquipmentToExerciseRequest(Equipment Equipment, bool Required = true);
