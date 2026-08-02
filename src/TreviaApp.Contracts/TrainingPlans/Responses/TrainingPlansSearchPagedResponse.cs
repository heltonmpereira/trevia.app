namespace TreviaApp.Contracts.TrainingPlans.Responses;

/// <summary>
/// Response payload for TrainingPlansSearchPagedResponse.
/// </summary>
/// <param name="TotalCount">Total Count value.</param>
/// <param name="Page">Page value.</param>
/// <param name="PageSize">Page Size value.</param>
/// <param name="TotalPages">Total Pages value.</param>
/// <param name="Items">Items value.</param>
public sealed record TrainingPlansSearchPagedResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyList<TrainingPlanSummaryResponse> Items);
