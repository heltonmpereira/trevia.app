using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

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
