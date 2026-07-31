namespace TreviaApp.Application.Authentication.Commands.ForgotPassword;
using TreviaApp.Application.Abstractions.Messaging;

public record ForgotPasswordCommand(string Email) : ICommand;
