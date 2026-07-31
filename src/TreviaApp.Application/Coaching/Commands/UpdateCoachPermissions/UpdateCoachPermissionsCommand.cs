namespace TreviaApp.Application.Coaching.Commands.UpdateCoachPermissions;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

public sealed record UpdateCoachPermissionsCommand(
    Guid LinkId,
    CoachPermissions Permissions)
    : ICommand<CoachStudentLinkResponse>;
