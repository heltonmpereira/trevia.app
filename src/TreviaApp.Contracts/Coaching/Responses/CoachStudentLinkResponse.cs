using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

/// <summary>
/// Response payload for CoachStudentLinkResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="CoachId">Coach Id value.</param>
/// <param name="CoachName">Coach Name value.</param>
/// <param name="CoachPhotoFileId">Coach Photo File Id value.</param>
/// <param name="StudentId">Student Id value.</param>
/// <param name="StudentName">Student Name value.</param>
/// <param name="StudentPhotoFileId">Student Photo File Id value.</param>
/// <param name="Permissions">Permissions value.</param>
/// <param name="IsActive">Is Active value.</param>
/// <param name="StartedAt">Started At value.</param>
/// <param name="EndedAt">Ended At value.</param>
/// <param name="EndReason">End Reason value.</param>
/// <param name="EndReasonNotes">End Reason Notes value.</param>
public sealed record CoachStudentLinkResponse(
    Guid Id,
    Guid CoachId,
    string CoachName,
    string? CoachPhotoFileId,
    Guid StudentId,
    string StudentName,
    string? StudentPhotoFileId,
    CoachPermissions Permissions,
    bool IsActive,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    CoachRelationshipEndReason? EndReason,
    string? EndReasonNotes);
