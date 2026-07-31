namespace TreviaApp.Application.Authentication.Queries.GetActiveSessions;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Authentication;

public record GetActiveSessionsQuery() : IQuery<UserSessionsResponse>;
