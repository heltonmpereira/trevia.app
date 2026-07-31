namespace TreviaApp.Application.Coaching.Queries.SearchCoachesNotLinked;

using TreviaApp.Contracts.Coaching.Responses;

public sealed record SearchCoachesNotLinkedQuery(
    string? SearchName = null,
    int Page = 1,
    int PageSize = 20) : IQuery<CoachStudentsPagedResponse>;
