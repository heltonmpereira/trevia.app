namespace TreviaApp.Application.Authentication.Commands.RefreshToken;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Authentication;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<AuthResponse>;
