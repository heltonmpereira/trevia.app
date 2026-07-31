namespace TreviaApp.Application.Exercises.Commands.AddEquipmentToExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

public sealed record AddEquipmentToExerciseCommand(
    Guid ExerciseId,
    Equipment Equipment,
    bool Required = true)
    : ICommand<ExerciseEquipmentResponse>;
