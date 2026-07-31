namespace TreviaApp.Application.Coaching.Commands.SendStudentRequestToCoach;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;

public sealed record SendStudentRequestToCoachCommand(
    Guid CoachId,
    string? Message,
    int ExpiresInDays = 30)
    : ICommand<CoachInviteResponse>;
