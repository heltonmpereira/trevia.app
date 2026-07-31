namespace TreviaApp.Application.Authentication.Commands.ChangePassword;
using TreviaApp.Application.Abstractions.Messaging;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmNewPassword) : ICommand;
