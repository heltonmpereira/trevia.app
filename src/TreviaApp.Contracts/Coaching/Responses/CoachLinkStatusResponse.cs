using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

/// <summary>
/// Response payload for CoachLinkStatusResponse.
/// </summary>
/// <param name="OtherUserId">Other User Id value.</param>
/// <param name="HasActiveLink">Has Active Link value.</param>
/// <param name="LinkId">Link Id value.</param>
/// <param name="IsCoachInRelationship">Is Coach In Relationship value.</param>
/// <param name="IsStudentInRelationship">Is Student In Relationship value.</param>
/// <param name="CurrentPermissions">Current Permissions value.</param>
/// <param name="PendingInviteStatus">Pending Invite Status value.</param>
/// <param name="PendingInviteId">Pending Invite Id value.</param>
/// <param name="PendingInviteDirection">Pending Invite Direction value.</param>
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
