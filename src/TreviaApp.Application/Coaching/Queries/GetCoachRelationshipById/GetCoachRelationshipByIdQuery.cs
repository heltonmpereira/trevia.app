namespace TreviaApp.Application.Coaching.Queries.GetCoachRelationshipById;

using TreviaApp.Contracts.Coaching.Responses;

public sealed record GetCoachRelationshipByIdQuery(Guid LinkId) : IQuery<CoachStudentLinkResponse>;
