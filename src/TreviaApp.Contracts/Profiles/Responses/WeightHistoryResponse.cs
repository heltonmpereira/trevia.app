namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record WeightHistoryResponse(
    int TotalCount,
    int Page,
    int PageSize,
    decimal? StartingWeightKg,
    decimal? LatestWeightKg,
    decimal? ChangeKg,
    IReadOnlyList<WeightEntryResponse> Entries);
