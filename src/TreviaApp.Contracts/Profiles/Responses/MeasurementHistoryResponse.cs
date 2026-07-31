namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record MeasurementHistoryResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<MeasurementResponse> Entries);
