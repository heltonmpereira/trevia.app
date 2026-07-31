namespace TreviaApp.Application.Authentication.Commands.ConfirmEmail;
using TreviaApp.Application.Abstractions.Messaging;

public record ConfirmEmailCommand(Guid UserId, string Token) : ICommand;
