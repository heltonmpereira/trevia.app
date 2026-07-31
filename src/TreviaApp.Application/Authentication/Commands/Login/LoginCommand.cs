namespace TreviaApp.Application.Authentication.Commands.Login;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Authentication;

public record LoginCommand(string Email, string Password, bool RememberMe = false) : ICommand<AuthResponse>;
