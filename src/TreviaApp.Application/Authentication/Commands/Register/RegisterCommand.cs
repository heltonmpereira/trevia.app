namespace TreviaApp.Application.Authentication.Commands.Register;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Authentication;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? DisplayName = null) : ICommand<AuthResponse>;
