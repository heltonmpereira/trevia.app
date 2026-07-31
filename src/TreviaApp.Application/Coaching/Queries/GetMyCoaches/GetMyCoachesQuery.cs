namespace TreviaApp.Application.Coaching.Queries.GetMyCoaches;

using TreviaApp.Contracts.Coaching.Responses;

public sealed record GetMyCoachesQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchName = null,
    bool? OnlyActive = true,
    string? SortBy = "linkedSinceDesc") : IQuery<CoachStudentsPagedResponse>;
