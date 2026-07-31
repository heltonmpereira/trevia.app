namespace TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record TrainingPlansSearchPagedResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyList<TrainingPlanSummaryResponse> Items);
