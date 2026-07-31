namespace TreviaApp.Application.Profiles.Queries.GetProfileByUserId;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record GetProfileByUserIdQuery(Guid TargetUserId) : IQuery<ProfileFullResponse>;
