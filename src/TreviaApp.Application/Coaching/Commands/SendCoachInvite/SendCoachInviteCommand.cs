namespace TreviaApp.Application.Coaching.Commands.SendCoachInvite;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

public sealed record SendCoachInviteCommand(
    Guid StudentId,
    string? Message,
    int ExpiresInDays = 30,
    CoachPermissions? GrantedPermissionsOnAccept = null)
    : ICommand<CoachInviteResponse>;
