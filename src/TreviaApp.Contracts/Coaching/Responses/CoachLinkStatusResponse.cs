using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

public sealed record CoachLinkStatusResponse(
    Guid OtherUserId,
    bool HasActiveLink,
    Guid? LinkId,
    bool IsCoachInRelationship,
    bool IsStudentInRelationship,
    CoachPermissions? CurrentPermissions,
    CoachRequestStatus? PendingInviteStatus,
    Guid? PendingInviteId,
    CoachInviteDirection? PendingInviteDirection);
