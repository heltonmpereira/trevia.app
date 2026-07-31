namespace TreviaApp.Application.Coaching.Queries.GetMyIncomingInvites;

using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

public sealed record GetMyIncomingInvitesQuery(
    int Page = 1,
    int PageSize = 10,
    CoachRequestStatus? StatusFilter = null,
    string? SortBy = "createdAtDesc") : IQuery<CoachingInvitesPagedResponse>;
