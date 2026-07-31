namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Exercises.Commands.AddEquipmentToExercise;
using TreviaApp.Application.Exercises.Commands.AddMediaToExercise;
using TreviaApp.Application.Exercises.Commands.AddMuscleToExercise;
using TreviaApp.Application.Exercises.Commands.ApproveExercise;
using TreviaApp.Application.Exercises.Commands.CreateExercise;
using TreviaApp.Application.Exercises.Commands.DeleteExercise;
using TreviaApp.Application.Exercises.Commands.RejectExercise;
using TreviaApp.Application.Exercises.Commands.RemoveEquipmentFromExercise;
using TreviaApp.Application.Exercises.Commands.RemoveMediaFromExercise;
using TreviaApp.Application.Exercises.Commands.RemoveMuscleFromExercise;
using TreviaApp.Application.Exercises.Commands.SetPrimaryMedia;
using TreviaApp.Application.Exercises.Commands.SubmitForApproval;
using TreviaApp.Application.Exercises.Commands.UpdateExercise;
using TreviaApp.Application.Exercises.Queries.GetAwaitingApprovalCount;
using TreviaApp.Application.Exercises.Queries.GetExerciseById;
using TreviaApp.Application.Exercises.Queries.GetMyExercises;
using TreviaApp.Application.Exercises.Queries.SearchAllExercises;
using TreviaApp.Application.Exercises.Queries.SearchApprovedExercises;
using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

[ApiController]
[Route("api/exercises")]
[Produces("application/json")]
[EnableRateLimiting("AuthEndpoint")]
public class ExercisesController : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ExerciseDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateExerciseRequest request, CancellationToken ct)
    {
        var command = new CreateExerciseCommand(
            request.Name,
            request.Environment,
            request.Modality,
            request.DifficultyLevel,
            request.MeasurementType,
            request.Instructions,
            request.ShortDescription,
            request.Tips,
            request.Tags,
            request.Visibility,
            request.Muscles?.Select(m => new MuscleMappingRequest(m.Muscle, m.Role, m.ActivationPercent)),
            request.Equipments?.Select(e => new EquipmentMappingRequest(e.Equipment, e.Required)));

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { exerciseId = result.Id }, result);
    }

    [HttpPut("{exerciseId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ExerciseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid exerciseId, [FromBody] UpdateExerciseRequest request, CancellationToken ct)
    {
        var command = new UpdateExerciseCommand(
            exerciseId,
            request.Name,
            request.Instructions,
            request.Environment,
            request.Modality,
            request.DifficultyLevel,
            request.MeasurementType,
            request.Visibility,
            request.ShortDescription,
            request.Tips,
            request.Tags);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{exerciseId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid exerciseId, CancellationToken ct)
    {
        await Sender.Send(new DeleteExerciseCommand(exerciseId), ct);
        return NoContent();
    }

    [HttpPost("{exerciseId:guid}/submit")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitForApproval([FromRoute] Guid exerciseId, CancellationToken ct)
    {
        await Sender.Send(new SubmitForApprovalCommand(exerciseId), ct);
        return NoContent();
    }

    [HttpPut("{exerciseId:guid}/approve")]
    [Authorize(Policy = AppPolicies.IsAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve([FromRoute] Guid exerciseId, CancellationToken ct)
    {
        await Sender.Send(new ApproveExerciseCommand(exerciseId), ct);
        return NoContent();
    }

    [HttpPut("{exerciseId:guid}/reject")]
    [Authorize(Policy = AppPolicies.IsAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject([FromRoute] Guid exerciseId, [FromBody] RejectExerciseRequest request, CancellationToken ct)
    {
        await Sender.Send(new RejectExerciseCommand(exerciseId, request.Reason), ct);
        return NoContent();
    }

    [HttpGet("{exerciseId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ExerciseDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid exerciseId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetExerciseByIdQuery(exerciseId), ct);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(ExerciseSearchPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ExerciseStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyExercisesQuery(page, pageSize, status), ct);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ExerciseSearchPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchApproved([FromQuery] SearchExercisesRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new SearchApprovedExercisesQuery(request), ct);
        return Ok(result);
    }

    [HttpGet("approved")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ExerciseSearchPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchApprovedAlias([FromQuery] SearchExercisesRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new SearchApprovedExercisesQuery(request), ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Policy = AppPolicies.CanModerateExercises)]
    [ProducesResponseType(typeof(ExerciseSearchPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAll(
        [FromQuery] SearchExercisesRequest filters,
        [FromQuery] bool includeDeleted = false,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new SearchAllExercisesQuery(filters, includeDeleted), ct);
        return Ok(result);
    }

    [HttpGet("awaiting-approval/count")]
    [Authorize(Policy = AppPolicies.CanModerateExercises)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAwaitingApprovalCount(CancellationToken ct)
    {
        var result = await Sender.Send(new GetAwaitingApprovalCountQuery(), ct);
        return Ok(result);
    }

    [HttpPost("{exerciseId:guid}/media")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExerciseMediaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMedia(
        [FromRoute] Guid exerciseId,
        IFormFile file,
        [FromForm] int order = 0,
        [FromForm] string? caption = null,
        [FromForm] bool isPrimary = false,
        [FromForm] Shared.Enums.MediaType? mediaType = null,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Arquivo não fornecido ou vazio.");
            return BadRequest(ModelState);
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var resolvedMediaType = mediaType ?? ResolveMediaType(file.ContentType);

        var command = new AddMediaToExerciseCommand(
            exerciseId,
            bytes,
            file.FileName,
            file.ContentType,
            file.Length,
            resolvedMediaType,
            order,
            caption,
            isPrimary);

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { exerciseId }, result);
    }

    [HttpDelete("{exerciseId:guid}/media/{mediaId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMedia([FromRoute] Guid exerciseId, [FromRoute] Guid mediaId, CancellationToken ct)
    {
        await Sender.Send(new RemoveMediaFromExerciseCommand(exerciseId, mediaId), ct);
        return NoContent();
    }

    [HttpPut("{exerciseId:guid}/media/{mediaId:guid}/primary")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryMedia([FromRoute] Guid exerciseId, [FromRoute] Guid mediaId, CancellationToken ct)
    {
        await Sender.Send(new SetPrimaryMediaCommand(exerciseId, mediaId), ct);
        return NoContent();
    }

    [HttpPost("{exerciseId:guid}/muscles")]
    [Authorize]
    [ProducesResponseType(typeof(ExerciseMuscleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMuscle([FromRoute] Guid exerciseId, [FromBody] AddMuscleToExerciseRequest request, CancellationToken ct)
    {
        var command = new AddMuscleToExerciseCommand(exerciseId, request.Muscle, request.Role, request.ActivationPercent);
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { exerciseId }, result);
    }

    [HttpDelete("{exerciseId:guid}/muscles/{muscle:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMuscle([FromRoute] Guid exerciseId, [FromRoute] int muscle, CancellationToken ct)
    {
        await Sender.Send(new RemoveMuscleFromExerciseCommand(exerciseId, (Shared.Enums.Muscle)muscle), ct);
        return NoContent();
    }

    [HttpPost("{exerciseId:guid}/equipments")]
    [Authorize]
    [ProducesResponseType(typeof(ExerciseEquipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddEquipment([FromRoute] Guid exerciseId, [FromBody] AddEquipmentToExerciseRequest request, CancellationToken ct)
    {
        var command = new AddEquipmentToExerciseCommand(exerciseId, request.Equipment, request.Required);
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { exerciseId }, result);
    }

    [HttpDelete("{exerciseId:guid}/equipments/{equipment:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveEquipment([FromRoute] Guid exerciseId, [FromRoute] int equipment, CancellationToken ct)
    {
        await Sender.Send(new RemoveEquipmentFromExerciseCommand(exerciseId, (Shared.Enums.Equipment)equipment), ct);
        return NoContent();
    }

    private static Shared.Enums.MediaType ResolveMediaType(string contentType)
    {
        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return Shared.Enums.MediaType.Image;
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return Shared.Enums.MediaType.Video;
        return Shared.Enums.MediaType.ExternalLink;
    }
}
