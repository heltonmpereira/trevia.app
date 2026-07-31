namespace TreviaApp.Application.Profiles.Queries.GetMyProfile;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record GetMyProfileQuery : IQuery<ProfileFullResponse>;
