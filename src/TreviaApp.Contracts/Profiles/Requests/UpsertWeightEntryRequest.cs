namespace TreviaApp.Contracts.Profiles.Requests;

/// <summary>
/// Request payload for UpsertWeightEntryRequest.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="WeightKg">Weight Kg value.</param>
/// <param name="MeasuredAt">Measured At value.</param>
/// <param name="Note">Note value.</param>
public sealed record UpsertWeightEntryRequest(
    long? Id,
    decimal WeightKg,
    DateTimeOffset MeasuredAt,
    string? Note = null);
