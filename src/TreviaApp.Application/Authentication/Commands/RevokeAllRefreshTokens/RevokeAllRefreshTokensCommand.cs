namespace TreviaApp.Application.Authentication.Commands.RevokeAllRefreshTokens;
using TreviaApp.Application.Abstractions.Messaging;

public record RevokeAllRefreshTokensCommand(Guid? TargetUserId = null, string Reason = "UserRevokedAll") : ICommand;
