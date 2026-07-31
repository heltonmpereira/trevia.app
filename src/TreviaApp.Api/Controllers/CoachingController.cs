namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Coaching.Commands.AcceptCoachInvite;
using TreviaApp.Application.Coaching.Commands.CancelCoachInvite;
using TreviaApp.Application.Coaching.Commands.EndCoachRelationship;
using TreviaApp.Application.Coaching.Commands.RejectCoachInvite;
using TreviaApp.Application.Coaching.Commands.SendCoachInvite;
using TreviaApp.Application.Coaching.Commands.SendStudentRequestToCoach;
using TreviaApp.Application.Coaching.Commands.UpdateCoachPermissions;
using TreviaApp.Application.Coaching.Queries.CheckCoachLinkStatus;
using TreviaApp.Application.Coaching.Queries.GetCoachRelationshipById;
using TreviaApp.Application.Coaching.Queries.GetCoachStudentsAsAdmin;
using TreviaApp.Application.Coaching.Queries.GetMyCoaches;
using TreviaApp.Application.Coaching.Queries.GetMyIncomingInvites;
using TreviaApp.Application.Coaching.Queries.GetMyOutgoingInvites;
using TreviaApp.Application.Coaching.Queries.GetMyStudents;
using TreviaApp.Application.Coaching.Queries.GetPendingCoachRequestsCount;
using TreviaApp.Application.Coaching.Queries.SearchCoachesNotLinked;
using TreviaApp.Application.Coaching.Queries.SearchStudentsNotLinked;
using TreviaApp.Contracts.Coaching.Requests;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

[ApiController]
[Route("api/coaching")]
[Produces("application/json")]
[EnableRateLimiting("AuthEndpoint")]
public class CoachingController : ApiControllerBase
{
    [HttpPost("invites/send-trainer")]
    [Authorize(Policy = AppPolicies.CanAssignTrainingPlans)]
    [ProducesResponseType(typeof(CoachInviteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SendCoachInvite([FromBody] SendCoachInviteRequest request, CancellationToken ct)
    {
        var command = new SendCoachInviteCommand(
            request.StudentId,
            request.Message,
            request.ExpiresInDays,
            request.GrantedPermissionsOnAccept);

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetRelationshipById), new { linkId = result.Id }, result);
    }

    [HttpPost("requests/send-student")]
    [Authorize(Policy = AppPolicies.IsStudent)]
    [ProducesResponseType(typeof(CoachInviteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SendStudentRequest([FromBody] SendStudentRequestRequest request, CancellationToken ct)
    {
        var command = new SendStudentRequestToCoachCommand(
            request.CoachId,
            request.Message,
            request.ExpiresInDays);

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetIncomingInvites), result);
    }

    [HttpPost("invites/{inviteId:guid}/accept")]
    [Authorize]
    [ProducesResponseType(typeof(CoachStudentLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptInvite([FromRoute] Guid inviteId, [FromBody] AcceptCoachInviteRequest? request, CancellationToken ct)
    {
        var result = await Sender.Send(new AcceptCoachInviteCommand(inviteId), ct);
        return Ok(result);
    }

    [HttpPost("invites/{inviteId:guid}/reject")]
    [Authorize]
    [ProducesResponseType(typeof(CoachInviteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectInvite([FromRoute] Guid inviteId, [FromBody] RejectCoachInviteRequest? request, CancellationToken ct)
    {
        var result = await Sender.Send(new RejectCoachInviteCommand(inviteId, request?.Reason), ct);
        return Ok(result);
    }

    [HttpPost("invites/{inviteId:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(CoachInviteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelInvite([FromRoute] Guid inviteId, CancellationToken ct)
    {
        var result = await Sender.Send(new CancelCoachInviteCommand(inviteId), ct);
        return Ok(result);
    }

    [HttpDelete("links/{linkId:guid}/end")]
    [Authorize]
    [ProducesResponseType(typeof(CoachStudentLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EndRelationship(
        [FromRoute] Guid linkId,
        [FromQuery] CoachRelationshipEndReason reason = CoachRelationshipEndReason.MutualAgreement,
        [FromQuery] string? notes = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new EndCoachRelationshipCommand(linkId, reason, notes), ct);
        return Ok(result);
    }

    [HttpPut("links/{linkId:guid}/permissions")]
    [Authorize]
    [ProducesResponseType(typeof(CoachStudentLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePermissions(
        [FromRoute] Guid linkId,
        [FromBody] UpdateCoachPermissionsRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateCoachPermissionsCommand(linkId, request.Permissions), ct);
        return Ok(result);
    }

    [HttpGet("invites/incoming")]
    [Authorize]
    [ProducesResponseType(typeof(CoachingInvitesPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIncomingInvites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CoachRequestStatus? statusFilter = null,
        [FromQuery] string? sortBy = "createdAtDesc",
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyIncomingInvitesQuery(page, pageSize, statusFilter, sortBy), ct);
        return Ok(result);
    }

    [HttpGet("invites/outgoing")]
    [Authorize]
    [ProducesResponseType(typeof(CoachingInvitesPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOutgoingInvites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CoachRequestStatus? statusFilter = null,
        [FromQuery] string? sortBy = "createdAtDesc",
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyOutgoingInvitesQuery(page, pageSize, statusFilter, sortBy), ct);
        return Ok(result);
    }

    [HttpGet("invites/pending-count")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingInvitesCount(CancellationToken ct)
    {
        var result = await Sender.Send(new GetPendingCoachRequestsCountQuery(), ct);
        return Ok(new { PendingCount = result });
    }

    [HttpGet("students")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(CoachStudentsPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] bool? onlyActive = true,
        [FromQuery] string? sortBy = "linkedSinceDesc",
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyStudentsQuery(page, pageSize, searchName, onlyActive, sortBy), ct);
        return Ok(result);
    }

    [HttpGet("coaches")]
    [Authorize]
    [ProducesResponseType(typeof(CoachStudentsPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyCoaches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] bool? onlyActive = true,
        [FromQuery] string? sortBy = "linkedSinceDesc",
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyCoachesQuery(page, pageSize, searchName, onlyActive, sortBy), ct);
        return Ok(result);
    }

    [HttpGet("links/{linkId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CoachStudentLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelationshipById([FromRoute] Guid linkId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetCoachRelationshipByIdQuery(linkId), ct);
        return Ok(result);
    }

    [HttpGet("link-status/{otherUserId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(CoachLinkStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckLinkStatus([FromRoute] Guid otherUserId, CancellationToken ct)
    {
        var result = await Sender.Send(new CheckCoachLinkStatusQuery(otherUserId), ct);
        return Ok(result);
    }

    [HttpGet("admin/coaches/{coachId:guid}/students")]
    [Authorize(Policy = AppPolicies.IsGymManagerOrAdmin)]
    [ProducesResponseType(typeof(CoachStudentsPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCoachStudentsAsAdmin(
        [FromRoute] Guid coachId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] bool onlyActive = true,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetCoachStudentsAsAdminQuery(coachId, page, pageSize, searchName, onlyActive), ct);
        return Ok(result);
    }

    [HttpGet("search/students")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(CoachStudentsPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchStudentsNotLinked(
        [FromQuery] string? searchName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new SearchStudentsNotLinkedQuery(searchName, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("search/coaches")]
    [Authorize]
    [ProducesResponseType(typeof(CoachStudentsPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchCoachesNotLinked(
        [FromQuery] string? searchName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new SearchCoachesNotLinkedQuery(searchName, page, pageSize), ct);
        return Ok(result);
    }
}
