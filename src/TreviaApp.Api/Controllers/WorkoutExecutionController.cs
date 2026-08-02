using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreviaApp.Api.Attributes;
using TreviaApp.Application.WorkoutExecution.Commands;
using TreviaApp.Application.WorkoutExecution.Commands.StartWorkoutSession;
using TreviaApp.Application.WorkoutExecution.Queries;
using TreviaApp.Contracts.WorkoutExecution.Requests;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Abstractions;
using TreviaApp.Infrastructure.Security;
using TreviaApp.Shared.Enums;
using static TreviaApp.Application.WorkoutExecution.Commands.ExerciseActions;
using static TreviaApp.Application.WorkoutExecution.Commands.PauseResumeFinish;
using static TreviaApp.Application.WorkoutExecution.Queries.Queries;

namespace TreviaApp.Api.Controllers;

[ApiController]
[Route("api/workouts")]
[Authorize]
[EnableRateLimiting("AuthEndpoint")]
[Produces("application/json")]
public class WorkoutExecutionController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;

    public WorkoutExecutionController(ISender sender, ICurrentUserService currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpPost("sessions/start/{trainingSessionId:guid}")]
    [ProducesResponseType(typeof(WorkoutSessionSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Start([FromRoute] Guid trainingSessionId, [FromBody] StartWorkoutSessionRequest? request = null, CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var cmd = new StartWorkoutSessionCommand(userId, trainingSessionId, request?.WeekNumberInPlan ?? 1);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { workoutSessionId = result.Value.Id }, result.Value)
            : this.FromResult(result);
    }

    [HttpPost("sessions/{workoutSessionId:guid}/pause")]
    [ProducesResponseType(typeof(WorkoutSessionSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pause([FromRoute] Guid workoutSessionId, CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var result = await _sender.Send(new PauseWorkoutSessionCommand(userId, workoutSessionId), ct);
        return this.FromResult(result);
    }

    [HttpPost("sessions/{workoutSessionId:guid}/resume")]
    [ProducesResponseType(typeof(WorkoutSessionSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resume([FromRoute] Guid workoutSessionId, CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var result = await _sender.Send(new ResumeWorkoutSessionCommand(userId, workoutSessionId), ct);
        return this.FromResult(result);
    }

    [HttpPost("sessions/{workoutSessionId:guid}/finish")]
    [ProducesResponseType(typeof(WorkoutSessionSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Finish(
        [FromRoute] Guid workoutSessionId,
        [FromBody] FinishWorkoutSessionRequest? request = null,
        CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var cmd = new FinishWorkoutSessionCommand(
            userId,
            workoutSessionId,
            request?.OverallRating,
            request?.GeneralNotes,
            request?.CaloriesBurned);
        var result = await _sender.Send(cmd, ct);
        return this.FromResult(result);
    }

    [HttpPost("sessions/{workoutSessionId:guid}/exercises/{workoutExerciseId:guid}/skip")]
    [ProducesResponseType(typeof(WorkoutExerciseResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SkipExercise(
        [FromRoute] Guid workoutSessionId,
        [FromRoute] Guid workoutExerciseId,
        [FromBody] SkipWorkoutExerciseRequest? request = null,
        CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var cmd = new SkipWorkoutExerciseCommand(userId, workoutSessionId, workoutExerciseId, request?.SkipReason);
        var result = await _sender.Send(cmd, ct);
        return this.FromResult(result);
    }

    [HttpPost("sessions/{workoutSessionId:guid}/exercises/{workoutExerciseId:guid}/extra-set")]
    [ProducesResponseType(typeof(WorkoutSetResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddExtraSet(
        [FromRoute] Guid workoutSessionId,
        [FromRoute] Guid workoutExerciseId,
        [FromBody] AddExtraSetRequest? request = null,
        CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var cmd = new AddExtraSetToExerciseCommand(userId, workoutSessionId, workoutExerciseId, request?.SuggestedSetNumber);
        var result = await _sender.Send(cmd, ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { workoutSessionId }, result.Value) : this.FromResult(result);
    }

    [HttpPut("sessions/{workoutSessionId:guid}/exercises/{workoutExerciseId:guid}/sets/{workoutSetId:guid}")]
    [ProducesResponseType(typeof(WorkoutSetResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogWorkoutSet(
        [FromRoute] Guid workoutSessionId,
        [FromRoute] Guid workoutExerciseId,
        [FromRoute] Guid workoutSetId,
        [FromBody] LogWorkoutSetRequest request,
        CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var cmd = new LogWorkoutSetCommand(
            userId,
            workoutSessionId,
            workoutExerciseId,
            workoutSetId,
            request.ActualReps,
            request.ActualLoadValue,
            request.ActualLoadUnit,
            request.ActualDurationSeconds,
            request.DistanceKm,
            request.SpeedKmh,
            request.InclinePercent,
            request.Calories,
            request.Completed,
            request.DifficultyRating,
            request.Notes);
        var result = await _sender.Send(cmd, ct);
        return this.FromResult(result);
    }

    [HttpGet("sessions")]
    [ProducesResponseType(typeof(WorkoutSessionsPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMy(
        [FromQuery] WorkoutStatus? statusFilter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? trainingPlanId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var q = new GetMyWorkoutSessionsQuery(userId, statusFilter, page, pageSize, trainingPlanId, from, to);
        var result = await _sender.Send(q, ct);
        return this.FromResult(result);
    }

    [HttpGet("sessions/current-active")]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentActive(CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var result = await _sender.Send(new GetCurrentActiveWorkoutSessionQuery(userId), ct);
        return this.FromResult(result);
    }

    [HttpGet("sessions/{workoutSessionId:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(WorkoutSessionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid workoutSessionId, CancellationToken ct = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var result = await _sender.Send(new GetWorkoutSessionByIdQuery(userId, workoutSessionId), ct);
        return this.FromResult(result);
    }
}
