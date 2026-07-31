namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record WeightEntryResponse(
    long Id,
    decimal WeightKg,
    DateTimeOffset MeasuredAt,
    string? Note,
    DateTimeOffset CreatedAt);
