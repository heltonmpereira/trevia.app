namespace TreviaApp.Contracts.Profiles.Requests;

public sealed record UpsertWeightEntryRequest(
    long? Id,
    decimal WeightKg,
    DateTimeOffset MeasuredAt,
    string? Note = null);
