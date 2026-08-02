namespace TreviaApp.Contracts.Coaching.Requests;

using TreviaApp.Shared.Enums;

/// <summary>
/// Request payload for SendCoachInviteRequest.
/// </summary>
/// <param name="StudentId">Student Id value.</param>
/// <param name="Message">Message value.</param>
/// <param name="ExpiresInDays">Expires In Days value.</param>
/// <param name="GrantedPermissionsOnAccept">Granted Permissions On Accept value.</param>
public sealed record SendCoachInviteRequest(
    Guid StudentId,
    string? Message = null,
    int ExpiresInDays = 30,
    CoachPermissions? GrantedPermissionsOnAccept = null);

/// <summary>
/// Request payload for SendStudentRequestRequest.
/// </summary>
/// <param name="CoachId">Coach Id value.</param>
/// <param name="Message">Message value.</param>
/// <param name="ExpiresInDays">Expires In Days value.</param>
public sealed record SendStudentRequestRequest(
    Guid CoachId,
    string? Message = null,
    int ExpiresInDays = 30);

/// <summary>
/// Request payload for AcceptCoachInviteRequest.
/// </summary>
/// <param name="AcceptanceNote">Acceptance Note value.</param>
public sealed record AcceptCoachInviteRequest(
    string? AcceptanceNote = null);

/// <summary>
/// Request payload for RejectCoachInviteRequest.
/// </summary>
/// <param name="Reason">Reason value.</param>
public sealed record RejectCoachInviteRequest(
    string? Reason = null);

/// <summary>
/// Request payload for UpdateCoachPermissionsRequest.
/// </summary>
/// <param name="Permissions">Permissions value.</param>
public sealed record UpdateCoachPermissionsRequest(
    CoachPermissions Permissions);
