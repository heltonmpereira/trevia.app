namespace TreviaApp.Application.Authentication.Commands.ResendConfirmationEmail;
using TreviaApp.Application.Abstractions.Messaging;

public record ResendConfirmationEmailCommand(Guid? UserId = null, string? Email = null) : ICommand;
