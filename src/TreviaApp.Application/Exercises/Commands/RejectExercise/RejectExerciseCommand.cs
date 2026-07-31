namespace TreviaApp.Application.Exercises.Commands.RejectExercise;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record RejectExerciseCommand(Guid ExerciseId, string Reason) : ICommand;
