namespace TreviaApp.Application.Coaching.Queries.SearchStudentsNotLinked;

using TreviaApp.Contracts.Coaching.Responses;

public sealed record SearchStudentsNotLinkedQuery(
    string? SearchName = null,
    int Page = 1,
    int PageSize = 20) : IQuery<CoachStudentsPagedResponse>;
