namespace TreviaApp.Application.Exercises.Commands.RemoveMediaFromExercise;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record RemoveMediaFromExerciseCommand(Guid ExerciseId, Guid MediaId) : ICommand;
