namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.TrainingPlans.Commands.AddExerciseToTrainingSession;
using TreviaApp.Application.TrainingPlans.Commands.AddSessionToTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.ArchiveTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.AssignToStudentTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.CompleteTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.CreateTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.DeleteTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.DuplicateTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.PauseTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.PublishTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.RemoveExerciseFromSession;
using TreviaApp.Application.TrainingPlans.Commands.RemoveTrainingSession;
using TreviaApp.Application.TrainingPlans.Commands.ReorderExercisesInSession;
using TreviaApp.Application.TrainingPlans.Commands.ReorderTrainingSessions;
using TreviaApp.Application.TrainingPlans.Commands.ResumeTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.UnpublishTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.UpdateExerciseInSession;
using TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingPlan;
using TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingSession;
using TreviaApp.Application.TrainingPlans.Commands.UpsertPrescriptionSetsInExercise;
using TreviaApp.Application.TrainingPlans.Queries.GetMyTrainingPlans;
using TreviaApp.Application.TrainingPlans.Queries.GetTrainingPlanById;
using TreviaApp.Application.TrainingPlans.Queries.GetTrainingPlansAssignedToStudent;
using TreviaApp.Application.TrainingPlans.Queries.SearchPublicTrainingPlanTemplates;
using TreviaApp.Contracts.TrainingPlans.Requests;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

[ApiController]
[Route("api/trainingplans")]
[Produces("application/json")]
[EnableRateLimiting("AuthEndpoint")]
public class TrainingPlansController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Policy = AppPolicies.CanCreateTrainingPlans)]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTrainingPlanRequest request, CancellationToken ct)
    {
        var command = new CreateTrainingPlanCommand(
            request.Name,
            request.Description,
            request.InstructionsIntro,
            request.NotesForStudent,
            request.SplitType,
            request.Visibility,
            request.TotalWeeks,
            request.SessionsPerWeek,
            request.TargetVolume,
            request.Tags);

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { planId = result.Id }, result);
    }

    [HttpPut("{planId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid planId, [FromBody] UpdateTrainingPlanRequest request, CancellationToken ct)
    {
        var command = new UpdateTrainingPlanCommand(
            planId,
            request.Name,
            request.Description,
            request.InstructionsIntro,
            request.NotesForStudent,
            request.SplitType,
            request.Visibility,
            request.TotalWeeks,
            request.SessionsPerWeek,
            request.TargetVolume,
            request.Tags);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{planId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid planId, CancellationToken ct)
    {
        await Sender.Send(new DeleteTrainingPlanCommand(planId), ct);
        return NoContent();
    }

    [HttpPost("{planId:guid}/duplicate")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate([FromRoute] Guid planId, [FromBody] DuplicatePlanRequest request, CancellationToken ct)
    {
        var command = new DuplicateTrainingPlanCommand(planId, request.NewName, request.AssignToMe);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{planId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Publish([FromRoute] Guid planId, [FromBody] PublishPlanRequest request, CancellationToken ct)
    {
        var command = new PublishTrainingPlanCommand(planId, request.AsPublicTemplate);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{planId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Unpublish([FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await Sender.Send(new UnpublishTrainingPlanCommand(planId), ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/assign/{studentId:guid}")]
    [Authorize(Policy = AppPolicies.CanAssignTrainingPlans)]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignToStudent([FromRoute] Guid planId, [FromRoute] Guid studentId, CancellationToken ct)
    {
        var command = new AssignToStudentTrainingPlanCommand(planId, studentId);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/pause")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pause([FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await Sender.Send(new PauseTrainingPlanCommand(planId), ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/resume")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resume([FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await Sender.Send(new ResumeTrainingPlanCommand(planId), ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/complete")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete([FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await Sender.Send(new CompleteTrainingPlanCommand(planId), ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/archive")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive([FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await Sender.Send(new ArchiveTrainingPlanCommand(planId), ct);
        return Ok(result);
    }

    [HttpPost("{planId:guid}/sessions")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSession([FromRoute] Guid planId, [FromBody] AddTrainingSessionRequest request, CancellationToken ct)
    {
        var command = new AddSessionToTrainingPlanCommand(
            planId,
            request.Name,
            request.Order,
            request.Description,
            request.SuggestedDayOfWeek,
            request.EstimatedDuration,
            request.CoachNotesInternal,
            request.Focus);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/sessions/{sessionId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSession([FromRoute] Guid planId, [FromRoute] Guid sessionId, [FromBody] UpdateTrainingSessionRequest request, CancellationToken ct)
    {
        var command = new UpdateTrainingSessionCommand(
            planId,
            sessionId,
            request.Name,
            request.Order,
            request.Description,
            request.SuggestedDayOfWeek,
            request.EstimatedDuration,
            request.CoachNotesInternal,
            request.Focus);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{planId:guid}/sessions/{sessionId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSession([FromRoute] Guid planId, [FromRoute] Guid sessionId, CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveTrainingSessionCommand(planId, sessionId), ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/sessions/reorder")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderSessions([FromRoute] Guid planId, [FromBody] ReorderSessionsRequest request, CancellationToken ct)
    {
        var command = new ReorderTrainingSessionsCommand(planId, request.Orders);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{planId:guid}/sessions/{sessionId:guid}/exercises")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddExerciseToSession([FromRoute] Guid planId, [FromRoute] Guid sessionId, [FromBody] AddExerciseToSessionRequest request, CancellationToken ct)
    {
        var command = new AddExerciseToTrainingSessionCommand(
            planId,
            sessionId,
            request.ExerciseId,
            request.Order,
            request.NotesForStudent,
            request.NotesForCoach,
            request.DefaultRestBetweenSetsSeconds,
            request.InitialSets);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/sessions/{sessionId:guid}/exercises/{sessionExerciseId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExerciseInSession([FromRoute] Guid planId, [FromRoute] Guid sessionId, [FromRoute] Guid sessionExerciseId, [FromBody] UpdateExerciseInSessionRequest request, CancellationToken ct)
    {
        var command = new UpdateExerciseInSessionCommand(
            planId,
            sessionId,
            sessionExerciseId,
            request.Order,
            request.NotesForStudent,
            request.NotesForCoach,
            request.DefaultRestBetweenSetsSeconds,
            request.GlobalTechnique);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{planId:guid}/sessions/{sessionId:guid}/exercises/{sessionExerciseId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveExerciseFromSession([FromRoute] Guid planId, [FromRoute] Guid sessionId, [FromRoute] Guid sessionExerciseId, CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveExerciseFromSessionCommand(planId, sessionId, sessionExerciseId), ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/sessions/{sessionId:guid}/exercises/reorder")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderExercisesInSession([FromRoute] Guid planId, [FromRoute] Guid sessionId, [FromBody] ReorderExercisesInSessionRequest request, CancellationToken ct)
    {
        var command = new ReorderExercisesInSessionCommand(planId, sessionId, request.Orders);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{planId:guid}/sessions/{sessionId:guid}/exercises/{sessionExerciseId:guid}/prescription-sets")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertPrescriptionSets([FromRoute] Guid planId, [FromRoute] Guid sessionId, [FromRoute] Guid sessionExerciseId, [FromBody] UpsertPrescriptionSetsRequest request, CancellationToken ct)
    {
        var command = new UpsertPrescriptionSetsInExerciseCommand(planId, sessionId, sessionExerciseId, request.Sets);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{planId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TrainingPlanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid planId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetTrainingPlanByIdQuery(planId), ct);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlansSearchPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] TrainingPlanStatus? statusFilter = null,
        [FromQuery] string? searchName = null,
        [FromQuery] string? sortBy = "createdAtDesc",
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyTrainingPlansQuery(page, pageSize, statusFilter, searchName, sortBy), ct);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TrainingPlansSearchPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPublicTemplates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? searchName = null,
        [FromQuery] TrainingSplitType? splitType = null,
        [FromQuery] DifficultyLevel? difficulty = null,
        [FromQuery] int? minSessions = null,
        [FromQuery] string? sortBy = "popularity",
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new SearchPublicTrainingPlanTemplatesQuery(page, pageSize, searchName, splitType, difficulty, minSessions, sortBy), ct);
        return Ok(result);
    }

    [HttpGet("assigned/student/{studentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(TrainingPlansSearchPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignedToStudent(
        [FromRoute] Guid studentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] TrainingPlanStatus? statusFilter = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetTrainingPlansAssignedToStudentQuery(studentId, page, pageSize, statusFilter), ct);
        return Ok(result);
    }
}
