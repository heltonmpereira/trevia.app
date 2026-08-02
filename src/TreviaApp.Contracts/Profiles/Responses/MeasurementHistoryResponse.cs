namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for MeasurementHistoryResponse.
/// </summary>
/// <param name="TotalCount">Total Count value.</param>
/// <param name="Page">Page value.</param>
/// <param name="PageSize">Page Size value.</param>
/// <param name="Entries">Entries value.</param>
public sealed record MeasurementHistoryResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<MeasurementResponse> Entries);
