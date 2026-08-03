namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Feedbacks;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Feedbacks.Requests;
using TreviaApp.Contracts.Feedbacks.Responses;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

[ApiController]
[Route("api/feedbacks")]
[Authorize]
[EnableRateLimiting("AuthEndpoint")]
[Produces("application/json")]
public class FeedbacksController : ApiControllerBase
{
    [HttpPost("workout-sessions/{sessionId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(WorkoutFeedbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateWorkoutFeedback(
        [FromRoute] Guid sessionId,
        [FromBody] CreateWorkoutFeedbackRequest request,
        CancellationToken ct = default)
    {
        var coachId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        var result = await Sender.Send(new CreateWorkoutFeedbackCommand(
            coachId, isStaff, sessionId, request.Text, request.Tone, request.IsPublic), ct);

        return CreatedAtAction(nameof(GetFeedbacksBySession), new { sessionId }, result);
    }

    [HttpPost("workout-exercises/{exerciseId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(ExerciseFeedbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateExerciseFeedback(
        [FromRoute] Guid exerciseId,
        [FromBody] CreateExerciseFeedbackRequest request,
        CancellationToken ct = default)
    {
        var coachId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        var result = await Sender.Send(new CreateExerciseFeedbackCommand(
            coachId, isStaff, exerciseId, request.Text, request.Tone, request.IsPublic), ct);

        return CreatedAtAction(nameof(GetFeedbacksBySession), new { sessionId = result.WorkoutSessionId }, result);
    }

    [HttpPost("workout-sets/{setId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(SetFeedbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSetFeedback(
        [FromRoute] Guid setId,
        [FromBody] CreateSetFeedbackRequest request,
        CancellationToken ct = default)
    {
        var coachId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        var result = await Sender.Send(new CreateSetFeedbackCommand(
            coachId, isStaff, setId, request.Text, request.Tone, request.IsPublic, request.MediaReferenceUrl), ct);

        return CreatedAtAction(nameof(GetFeedbacksBySession), new { sessionId = result.WorkoutSessionId }, result);
    }

    [HttpPut("{feedbackId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UnifiedFeedbackItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeedback(
        [FromRoute] Guid feedbackId,
        [FromQuery] FeedbackLevel level,
        [FromBody] UpdateFeedbackRequest request,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        var result = await Sender.Send(new UpdateFeedbackCommand(
            userId, isStaff, level, feedbackId, request.Text, request.Tone, request.IsPublic, request.MediaReferenceUrl), ct);

        return Ok(result);
    }

    [HttpDelete("{feedbackId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeedback(
        [FromRoute] Guid feedbackId,
        [FromQuery] FeedbackLevel level,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        await Sender.Send(new DeleteFeedbackCommand(userId, isStaff, level, feedbackId), ct);
        return NoContent();
    }

    [HttpGet("workout-sessions/{sessionId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(FeedbacksBySessionBundleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeedbacksBySession(
        [FromRoute] Guid sessionId,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        var result = await Sender.Send(new GetFeedbacksBySessionQuery(userId, isStaff, sessionId), ct);
        return Ok(result);
    }

    [HttpPut("{feedbackId:guid}/read")]
    [Authorize(Policy = AppPolicies.IsStudent)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkFeedbackAsRead(
        [FromRoute] Guid feedbackId,
        [FromQuery] FeedbackLevel level,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        await Sender.Send(new MarkFeedbackReadCommand(userId, level, feedbackId), ct);
        return NoContent();
    }

    [HttpGet("my")]
    [Authorize(Policy = AppPolicies.IsStudent)]
    [ProducesResponseType(typeof(PaginatedResponse<UnifiedFeedbackItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyFeedbacks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? workoutSessionId = null,
        [FromQuery] bool? onlyUnread = null,
        [FromQuery] FeedbackLevel? level = null,
        CancellationToken ct = default)
    {
        var studentId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyFeedbacksQuery(
            studentId, page, pageSize, workoutSessionId, onlyUnread, level), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(PaginatedResponse<UnifiedFeedbackItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentFeedbacks(
        [FromRoute] Guid studentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? workoutSessionId = null,
        [FromQuery] FeedbackLevel? level = null,
        CancellationToken ct = default)
    {
        var viewerId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);

        var result = await Sender.Send(new GetStudentFeedbacksQuery(
            viewerId, isStaff, studentId, page, pageSize, workoutSessionId, level), ct);
        return Ok(result);
    }

    [HttpPost("exercise-feedbacks/{feedbackId:guid}/respond")]
    [Authorize(Policy = AppPolicies.IsStudent)]
    [ProducesResponseType(typeof(ExerciseFeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RespondToExerciseFeedback(
        [FromRoute] Guid feedbackId,
        [FromBody] RespondToExerciseFeedbackRequest request,
        CancellationToken ct = default)
    {
        var studentId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new RespondToExerciseFeedbackCommand(studentId, feedbackId, request.ResponseText), ct);
        return Ok(result);
    }
}
