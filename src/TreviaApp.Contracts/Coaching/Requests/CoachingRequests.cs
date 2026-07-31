namespace TreviaApp.Contracts.Coaching.Requests;

using TreviaApp.Shared.Enums;

public sealed record SendCoachInviteRequest(
    Guid StudentId,
    string? Message = null,
    int ExpiresInDays = 30,
    CoachPermissions? GrantedPermissionsOnAccept = null);

public sealed record SendStudentRequestRequest(
    Guid CoachId,
    string? Message = null,
    int ExpiresInDays = 30);

public sealed record AcceptCoachInviteRequest(
    string? AcceptanceNote = null);

public sealed record RejectCoachInviteRequest(
    string? Reason = null);

public sealed record UpdateCoachPermissionsRequest(
    CoachPermissions Permissions);
