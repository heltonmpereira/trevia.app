namespace TreviaApp.Application.Exercises.Commands.SubmitForApproval;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record SubmitForApprovalCommand(Guid ExerciseId) : ICommand;
