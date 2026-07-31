namespace TreviaApp.Application.Consents.Commands.RevokeConsent;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Shared.Enums;

public record RevokeConsentCommand(ConsentType ConsentType, string? Reason) : ICommand;
