namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Profiles.Commands.CreateProfile;
using TreviaApp.Application.Profiles.Commands.DeleteMeasurement;
using TreviaApp.Application.Profiles.Commands.DeleteProfile;
using TreviaApp.Application.Profiles.Commands.DeleteWeightEntry;
using TreviaApp.Application.Profiles.Commands.RemoveProfilePhoto;
using TreviaApp.Application.Profiles.Commands.UpdateEquipments;
using TreviaApp.Application.Profiles.Commands.UpdateProfile;
using TreviaApp.Application.Profiles.Commands.UploadProfilePhoto;
using TreviaApp.Application.Profiles.Commands.UpsertMeasurement;
using TreviaApp.Application.Profiles.Commands.UpsertWeightEntry;
using TreviaApp.Application.Profiles.Queries.GetMeasurementHistory;
using TreviaApp.Application.Profiles.Queries.GetMyProfile;
using TreviaApp.Application.Profiles.Queries.GetProfileByUserId;
using TreviaApp.Application.Profiles.Queries.GetWeightHistory;
using TreviaApp.Contracts.Profiles.Requests;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Shared.Enums;

[ApiController]
[Route("api/profiles")]
[Produces("application/json")]
[EnableRateLimiting("AuthEndpoint")]
[Authorize]
public class ProfilesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProfileFullResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request, CancellationToken ct)
    {
        var command = new CreateProfileCommand(
            request.Goal,
            request.Experience,
            request.PreferredEnvironment,
            request.PrivacyLevel ?? PrivacyLevel.Private,
            request.PreferredUnits ?? "Metric",
            request.Bio);

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetMyProfile), new { }, result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProfileFullResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateProfileCommand(
            request.PrivacyLevel,
            request.Goal,
            request.Experience,
            request.PreferredEnvironment,
            request.PreferredUnits,
            request.Bio);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileFullResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await Sender.Send(new GetMyProfileQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(ProfileFullResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileByUserId([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetProfileByUserIdQuery(userId), ct);
        return Ok(result);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile(CancellationToken ct)
    {
        await Sender.Send(new DeleteProfileCommand(), ct);
        return NoContent();
    }

    [HttpPost("weight")]
    [ProducesResponseType(typeof(WeightEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertWeightEntry([FromBody] UpsertWeightEntryRequest request, CancellationToken ct)
    {
        var command = new UpsertWeightEntryCommand(
            request.WeightKg,
            request.MeasuredAt,
            request.Note);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("weight")]
    [ProducesResponseType(typeof(WeightHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWeightHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetWeightHistoryQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpDelete("weight/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWeightEntry([FromRoute] Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteWeightEntryCommand(id), ct);
        return NoContent();
    }

    [HttpPost("measurements")]
    [ProducesResponseType(typeof(MeasurementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertMeasurement([FromBody] UpsertMeasurementRequest request, CancellationToken ct)
    {
        var command = new UpsertMeasurementCommand(
            request.MeasuredAt,
            request.HeightCm,
            request.WaistCm,
            request.HipCm,
            request.ChestCm,
            request.ArmLeftCm,
            request.ArmRightCm,
            request.ThighLeftCm,
            request.ThighRightCm,
            request.CalfLeftCm,
            request.CalfRightCm,
            request.BodyFatPercent,
            request.WaterPercent,
            request.MuscleMassPercent,
            request.VisceralFatRating,
            request.BmiKgM2,
            request.Note);

        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("measurements")]
    [ProducesResponseType(typeof(MeasurementHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMeasurementHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMeasurementHistoryQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpDelete("measurements/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMeasurement([FromRoute] Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteMeasurementCommand(id), ct);
        return NoContent();
    }

    [HttpPost("photo")]
    [ProducesResponseType(typeof(PhotoUploadResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadProfilePhoto([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length <= 0)
        {
            ModelState.AddModelError("file", "Arquivo não fornecido ou vazio.");
            return BadRequest(ModelState);
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var command = new UploadProfilePhotoCommand(bytes, file.FileName, file.ContentType, file.Length);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("photo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProfilePhoto(CancellationToken ct)
    {
        await Sender.Send(new RemoveProfilePhotoCommand(), ct);
        return NoContent();
    }

    [HttpPut("equipments")]
    [ProducesResponseType(typeof(List<Equipment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEquipments([FromBody] UpdateEquipmentsRequest req, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateEquipmentsCommand(req.Equipments), ct);
        return Ok(result);
    }
}
