namespace TreviaApp.Application.Exercises.Commands.ApproveExercise;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record ApproveExerciseCommand(Guid ExerciseId) : ICommand;
