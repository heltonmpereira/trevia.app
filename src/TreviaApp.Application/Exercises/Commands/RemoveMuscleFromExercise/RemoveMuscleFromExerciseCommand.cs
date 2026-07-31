namespace TreviaApp.Application.Exercises.Commands.RemoveMuscleFromExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Shared.Enums;

public sealed record RemoveMuscleFromExerciseCommand(Guid ExerciseId, Muscle Muscle) : ICommand;
