namespace TreviaApp.Application.Profiles.Commands.UpsertWeightEntry;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record UpsertWeightEntryCommand(
    decimal WeightKg,
    DateTimeOffset MeasuredAt,
    string? Note) : ICommand<WeightEntryResponse>;
