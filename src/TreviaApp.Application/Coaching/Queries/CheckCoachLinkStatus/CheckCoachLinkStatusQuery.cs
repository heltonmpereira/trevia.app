namespace TreviaApp.Application.Coaching.Queries.CheckCoachLinkStatus;

using TreviaApp.Contracts.Coaching.Responses;

public sealed record CheckCoachLinkStatusQuery(Guid OtherUserId) : IQuery<CoachLinkStatusResponse>;
