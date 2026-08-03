using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Reports.Queries;
using TreviaApp.Contracts.Reports.Responses;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;
using static TreviaApp.Application.Reports.Queries.ReportQueries;

namespace TreviaApp.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
[EnableRateLimiting("AuthEndpoint")]
[Produces("application/json")]
public class ReportsController : ApiControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(WorkoutSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMySummary(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] Guid? trainingPlanId = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyWorkoutSummaryQuery(userId, from, to, trainingPlanId), ct);
        return Ok(result);
    }

    [HttpGet("calendar")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkoutCalendarDayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyCalendar(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyWorkoutCalendarQuery(userId, year, month), ct);
        return Ok(result);
    }

    [HttpGet("progress")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkoutProgressPointResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyProgress(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] ProgressGranularity granularity = ProgressGranularity.Week,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyProgressOverTimeQuery(userId, from, to, granularity), ct);
        return Ok(result);
    }

    [HttpGet("muscles")]
    [ProducesResponseType(typeof(IReadOnlyList<MuscleVolumeItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyMuscleDistribution(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyMuscleVolumeDistributionQuery(userId, from, to), ct);
        return Ok(result);
    }

    [HttpGet("exercises/top")]
    [ProducesResponseType(typeof(IReadOnlyList<ExerciseRankItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTopExercises(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int top = 10,
        [FromQuery] ExerciseRankBy rankBy = ExerciseRankBy.Volume,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyMostPerformedExercisesQuery(userId, from, to, top, rankBy), ct);
        return Ok(result);
    }

    [HttpGet("records")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonalRecordItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyRecords(
        [FromQuery] Guid? exerciseId = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyPersonalRecordsQuery(userId, exerciseId), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}/summary")]
    [ProducesResponseType(typeof(WorkoutSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentSummary(
        [FromRoute] Guid studentId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] Guid? trainingPlanId = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);
        var result = await Sender.Send(new GetStudentWorkoutSummaryQuery(userId, studentId, isStaff, from, to, trainingPlanId), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}/calendar")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkoutCalendarDayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentCalendar(
        [FromRoute] Guid studentId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);
        var result = await Sender.Send(new GetStudentWorkoutCalendarQuery(userId, studentId, isStaff, year, month), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}/progress")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkoutProgressPointResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentProgress(
        [FromRoute] Guid studentId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] ProgressGranularity granularity = ProgressGranularity.Week,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);
        var result = await Sender.Send(new GetStudentProgressOverTimeQuery(userId, studentId, isStaff, from, to, granularity), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}/muscles")]
    [ProducesResponseType(typeof(IReadOnlyList<MuscleVolumeItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentMuscleDistribution(
        [FromRoute] Guid studentId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);
        var result = await Sender.Send(new GetStudentMuscleVolumeDistributionQuery(userId, studentId, isStaff, from, to), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}/exercises/top")]
    [ProducesResponseType(typeof(IReadOnlyList<ExerciseRankItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentTopExercises(
        [FromRoute] Guid studentId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int top = 10,
        [FromQuery] ExerciseRankBy rankBy = ExerciseRankBy.Volume,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);
        var result = await Sender.Send(new GetStudentMostPerformedExercisesQuery(userId, studentId, isStaff, from, to, top, rankBy), ct);
        return Ok(result);
    }

    [HttpGet("students/{studentId:guid}/records")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonalRecordItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentRecords(
        [FromRoute] Guid studentId,
        [FromQuery] Guid? exerciseId = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isStaff = CurrentUser.IsInRole(AppRoles.Administrator) || CurrentUser.IsInRole(AppRoles.GymManager);
        var result = await Sender.Send(new GetStudentPersonalRecordsQuery(userId, studentId, isStaff, exerciseId), ct);
        return Ok(result);
    }
}
