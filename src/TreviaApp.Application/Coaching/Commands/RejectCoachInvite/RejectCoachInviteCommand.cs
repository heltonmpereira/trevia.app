namespace TreviaApp.Application.Coaching.Commands.RejectCoachInvite;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;

public sealed record RejectCoachInviteCommand(
    Guid InviteId,
    string? Reason = null)
    : ICommand<CoachInviteResponse>;
