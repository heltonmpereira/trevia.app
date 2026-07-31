namespace TreviaApp.Application.Authentication.Commands.ResetPassword;
using TreviaApp.Application.Abstractions.Messaging;

public record ResetPasswordCommand(Guid UserId, string Token, string Password, string ConfirmPassword) : ICommand;
