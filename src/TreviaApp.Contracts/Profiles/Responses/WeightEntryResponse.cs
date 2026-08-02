namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for WeightEntryResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="WeightKg">Weight Kg value.</param>
/// <param name="MeasuredAt">Measured At value.</param>
/// <param name="Note">Note value.</param>
/// <param name="CreatedAt">Created At value.</param>
public sealed record WeightEntryResponse(
    long Id,
    decimal WeightKg,
    DateTimeOffset MeasuredAt,
    string? Note,
    DateTimeOffset CreatedAt);
