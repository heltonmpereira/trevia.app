namespace TreviaApp.Application.Exercises.Commands.RemoveEquipmentFromExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Shared.Enums;

public sealed record RemoveEquipmentFromExerciseCommand(Guid ExerciseId, Equipment Equipment) : ICommand;
