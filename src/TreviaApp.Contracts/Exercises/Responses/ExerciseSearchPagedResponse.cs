namespace TreviaApp.Contracts.Exercises.Responses;

public sealed record ExerciseSearchPagedResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyList<ExerciseSummaryResponse> Items);
