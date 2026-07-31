namespace TreviaApp.Application.Coaching.Commands.CancelCoachInvite;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;

public sealed record CancelCoachInviteCommand(
    Guid InviteId)
    : ICommand<CoachInviteResponse>;
