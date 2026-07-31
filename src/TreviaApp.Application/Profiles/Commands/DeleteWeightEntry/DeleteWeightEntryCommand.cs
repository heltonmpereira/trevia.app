namespace TreviaApp.Application.Profiles.Commands.DeleteWeightEntry;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record DeleteWeightEntryCommand(Guid WeightEntryId) : ICommand;
