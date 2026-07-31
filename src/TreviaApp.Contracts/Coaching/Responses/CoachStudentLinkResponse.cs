using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

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
