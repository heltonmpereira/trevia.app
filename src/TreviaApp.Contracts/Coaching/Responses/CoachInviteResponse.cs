using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

/// <summary>
/// Response payload for CoachInviteResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="CoachId">Coach Id value.</param>
/// <param name="CoachName">Coach Name value.</param>
/// <param name="CoachPhotoFileId">Coach Photo File Id value.</param>
/// <param name="StudentId">Student Id value.</param>
/// <param name="StudentName">Student Name value.</param>
/// <param name="StudentPhotoFileId">Student Photo File Id value.</param>
/// <param name="Direction">Direction value.</param>
/// <param name="Status">Status value.</param>
/// <param name="Message">Message value.</param>
/// <param name="ExpiresAt">Expires At value.</param>
/// <param name="IsExpired">Is Expired value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="RespondedAt">Responded At value.</param>
/// <param name="GrantedPermissionsOnAccept">Granted Permissions On Accept value.</param>
/// <param name="RejectionReason">Rejection Reason value.</param>
public sealed record CoachInviteResponse(
    Guid Id,
    Guid CoachId,
    string CoachName,
    string? CoachPhotoFileId,
    Guid StudentId,
    string StudentName,
    string? StudentPhotoFileId,
    CoachInviteDirection Direction,
    CoachRequestStatus Status,
    string? Message,
    DateTimeOffset ExpiresAt,
    bool IsExpired,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RespondedAt,
    CoachPermissions GrantedPermissionsOnAccept,
    string? RejectionReason);
