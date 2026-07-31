namespace TreviaApp.Application.Coaching.Mappings;

using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;

public static class CoachingMappings
{
    public static CoachInviteResponse MapInvite(
        CoachStudentRequest request,
        string coachName,
        string? coachPhotoFileId,
        string studentName,
        string? studentPhotoFileId)
    {
        var isExpired = request.IsExpired || DateTimeOffset.UtcNow > request.ExpiresAt;

        return new CoachInviteResponse(
            request.Id,
            request.CoachId,
            coachName,
            coachPhotoFileId,
            request.StudentId,
            studentName,
            studentPhotoFileId,
            request.Direction,
            request.Status,
            request.Message,
            request.ExpiresAt,
            isExpired,
            request.CreatedAt,
            request.RespondedAt,
            request.GrantedPermissionsOnAccept,
            request.RejectionReason);
    }

    public static CoachStudentLinkResponse MapLink(
        CoachStudentLink link,
        string coachName,
        string? coachPhotoFileId,
        string studentName,
        string? studentPhotoFileId)
    {
        return new CoachStudentLinkResponse(
            link.Id,
            link.CoachId,
            coachName,
            coachPhotoFileId,
            link.StudentId,
            studentName,
            studentPhotoFileId,
            link.Permissions,
            link.IsActive,
            link.StartedAt,
            link.EndedAt,
            link.EndReason,
            link.EndReasonNotes);
    }
}
