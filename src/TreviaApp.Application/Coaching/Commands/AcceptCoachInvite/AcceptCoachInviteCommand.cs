namespace TreviaApp.Application.Coaching.Commands.AcceptCoachInvite;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;

public sealed record AcceptCoachInviteCommand(
    Guid InviteId)
    : ICommand<CoachStudentLinkResponse>;
