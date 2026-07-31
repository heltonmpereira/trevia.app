namespace TreviaApp.Application.Authentication.Commands.RevokeRefreshToken;
using TreviaApp.Application.Abstractions.Messaging;

public record RevokeRefreshTokenCommand(string RefreshToken, string Reason = "UserRevoked") : ICommand;
