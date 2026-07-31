namespace TreviaApp.Application.Coaching.Commands.EndCoachRelationship;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

public sealed record EndCoachRelationshipCommand(
    Guid LinkId,
    CoachRelationshipEndReason Reason,
    string? Notes = null)
    : ICommand<CoachStudentLinkResponse>;
