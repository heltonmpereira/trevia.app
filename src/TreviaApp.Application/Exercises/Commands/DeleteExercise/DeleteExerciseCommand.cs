namespace TreviaApp.Application.Exercises.Commands.DeleteExercise;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record DeleteExerciseCommand(Guid ExerciseId) : ICommand;
