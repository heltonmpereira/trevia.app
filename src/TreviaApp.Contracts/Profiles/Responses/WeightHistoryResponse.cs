namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for WeightHistoryResponse.
/// </summary>
/// <param name="TotalCount">Total Count value.</param>
/// <param name="Page">Page value.</param>
/// <param name="PageSize">Page Size value.</param>
/// <param name="StartingWeightKg">Starting Weight Kg value.</param>
/// <param name="LatestWeightKg">Latest Weight Kg value.</param>
/// <param name="ChangeKg">Change Kg value.</param>
/// <param name="Entries">Entries value.</param>
public sealed record WeightHistoryResponse(
    int TotalCount,
    int Page,
    int PageSize,
    decimal? StartingWeightKg,
    decimal? LatestWeightKg,
    decimal? ChangeKg,
    IReadOnlyList<WeightEntryResponse> Entries);
