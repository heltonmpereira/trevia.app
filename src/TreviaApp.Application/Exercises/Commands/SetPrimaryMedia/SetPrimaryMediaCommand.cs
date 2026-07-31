namespace TreviaApp.Application.Exercises.Commands.SetPrimaryMedia;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record SetPrimaryMediaCommand(Guid ExerciseId, Guid MediaId) : ICommand;
