namespace TreviaApp.Application.Authentication.Queries.GetCurrentUser;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Authentication;

public record GetCurrentUserQuery() : IQuery<CurrentUserResponse>;
