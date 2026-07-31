namespace TreviaApp.Application.Coaching.Queries.GetMyOutgoingInvites;

using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

public sealed record GetMyOutgoingInvitesQuery(
    int Page = 1,
    int PageSize = 10,
    CoachRequestStatus? StatusFilter = null,
    string? SortBy = "createdAtDesc") : IQuery<CoachingInvitesPagedResponse>;
