namespace TreviaApp.Application.Coaching.Queries.GetCoachStudentsAsAdmin;

using TreviaApp.Contracts.Coaching.Responses;

public sealed record GetCoachStudentsAsAdminQuery(
    Guid CoachId,
    int Page = 1,
    int PageSize = 10,
    string? SearchName = null,
    bool? OnlyActive = true) : IQuery<CoachStudentsPagedResponse>;
