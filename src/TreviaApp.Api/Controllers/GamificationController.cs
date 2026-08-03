namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Gamification;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Gamification.Requests;
using TreviaApp.Contracts.Gamification.Responses;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

[ApiController]
[Route("api/gamification")]
[Authorize]
[EnableRateLimiting("AuthEndpoint")]
[Produces("application/json")]
public class GamificationController : ApiControllerBase
{
    #region ===== POINTS =====

    [HttpGet("points/balance")]
    [ProducesResponseType(typeof(PointBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPointsBalance(
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetPointBalanceQuery(userId, false), ct);
        return Ok(result);
    }

    [HttpGet("points/balance/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(PointBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentPointsBalance(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var isAdmin = CurrentUser.IsInRole(AppRoles.Administrator);
        var result = await Sender.Send(new GetPointBalanceQuery(requesterId, true, userId), ct);
        return Ok(result);
    }

    [HttpGet("points/history")]
    [ProducesResponseType(typeof(PaginatedResponse<PointHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPointsHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PointReason? reason = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetPointHistoryQuery(userId, false, null, page, pageSize, reason), ct);
        return Ok(result);
    }

    [HttpGet("points/history/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(PaginatedResponse<PointHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentPointsHistory(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PointReason? reason = null,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetPointHistoryQuery(requesterId, true, userId, page, pageSize, reason), ct);
        return Ok(result);
    }

    [HttpPost("points/award/workout/{sessionId:guid}")]
    [Authorize(Roles = AppRoles.Student + "," + AppRoles.Administrator)]
    [ProducesResponseType(typeof(AwardWorkoutPointsResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AwardWorkoutPoints(
        [FromRoute] Guid sessionId,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var isAdmin = CurrentUser.IsInRole(AppRoles.Administrator);
        var result = await Sender.Send(new AwardWorkoutPointsCommand(userId, isAdmin, sessionId), ct);
        return Ok(result);
    }

    [HttpPost("points/adjust")]
    [Authorize(Roles = AppRoles.Administrator)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustPoints(
        [FromBody] AdjustPointsRequest request,
        [FromQuery] Guid targetUserId,
        CancellationToken ct = default)
    {
        var adminId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new AdjustPointsCommand(
            adminId, targetUserId, request.Amount, request.Description), ct);
        return Ok(result);
    }

    #endregion

    #region ===== PROGRESS / LEVEL =====

    [HttpGet("progress")]
    [ProducesResponseType(typeof(UserLevelProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyLevelProgress(
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetUserLevelProgressQuery(userId, false), ct);
        return Ok(result);
    }

    [HttpGet("progress/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(UserLevelProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentLevelProgress(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetUserLevelProgressQuery(requesterId, true, userId), ct);
        return Ok(result);
    }

    #endregion

    #region ===== ACHIEVEMENTS =====

    [HttpGet("achievements")]
    [ProducesResponseType(typeof(PaginatedResponse<AchievementProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAchievementsWithProgress(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetAchievementsWithProgressQuery(userId, false, null, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("achievements/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(PaginatedResponse<AchievementProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentAchievements(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetAchievementsWithProgressQuery(requesterId, true, userId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("achievements/recent")]
    [ProducesResponseType(typeof(List<AchievementProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRecentAchievements(
        [FromQuery] int top = 5,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetRecentAchievementsQuery(userId, false, null, top), ct);
        return Ok(result);
    }

    [HttpGet("achievements/recent/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(List<AchievementProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentRecentAchievements(
        [FromRoute] Guid userId,
        [FromQuery] int top = 5,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetRecentAchievementsQuery(requesterId, true, userId, top), ct);
        return Ok(result);
    }

    #endregion

    #region ===== STREAKS =====

    [HttpGet("streaks")]
    [ProducesResponseType(typeof(StreaksSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyStreaks(
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetStreaksSummaryQuery(userId, false), ct);
        return Ok(result);
    }

    [HttpGet("streaks/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(StreaksSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentStreaks(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetStreaksSummaryQuery(requesterId, true, userId), ct);
        return Ok(result);
    }

    [HttpPost("streaks/recompute")]
    [Authorize(Roles = AppRoles.Student + "," + AppRoles.Administrator)]
    [ProducesResponseType(typeof(RecomputeStreaksResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RecomputeStreaks(
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new RecomputeStreaksCommand(userId), ct);
        return Ok(result);
    }

    #endregion

    #region ===== MISSIONS =====

    [HttpGet("missions/today")]
    [ProducesResponseType(typeof(List<UserMissionProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyTodayMissions(
        [FromQuery] DateTime? date = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetTodayMissionsQuery(userId, false, null, date), ct);
        return Ok(result);
    }

    [HttpGet("missions/today/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(List<UserMissionProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentTodayMissions(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? date = null,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetTodayMissionsQuery(requesterId, true, userId, date), ct);
        return Ok(result);
    }

    [HttpGet("missions/this-week")]
    [ProducesResponseType(typeof(List<UserMissionProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyThisWeekMissions(
        [FromQuery] DateTime? weekStart = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetThisWeekMissionsQuery(userId, false, null, weekStart), ct);
        return Ok(result);
    }

    [HttpGet("missions/this-week/users/{userId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(List<UserMissionProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStudentThisWeekMissions(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? weekStart = null,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetThisWeekMissionsQuery(requesterId, true, userId, weekStart), ct);
        return Ok(result);
    }

    [HttpPost("missions/{missionId:guid}/claim")]
    [Authorize(Roles = AppRoles.Student + "," + AppRoles.Administrator)]
    [ProducesResponseType(typeof(ClaimMissionResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimMissionReward(
        [FromRoute] Guid missionId,
        [FromQuery] string type = "Daily",
        [FromQuery] DateTime? date = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new ClaimMissionCommand(userId, missionId, type, date), ct);
        if (!result.Success && !string.IsNullOrEmpty(result.Error))
            return BadRequest(result.Error);
        return Ok(result);
    }

    #endregion

    #region ===== DASHBOARD =====

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(GamificationDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyDashboard(
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetGamificationDashboardQuery(userId), ct);
        return Ok(result);
    }

    [HttpGet("dashboard/students/{studentId:guid}")]
    [Authorize(Policy = AppPolicies.IsTrainerOrAdmin)]
    [ProducesResponseType(typeof(GamificationDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentDashboard(
        [FromRoute] Guid studentId,
        CancellationToken ct = default)
    {
        var requesterId = CurrentUser.UserId!.Value;
        var isAdmin = CurrentUser.IsInRole(AppRoles.Administrator);
        var result = await Sender.Send(new GetStudentGamificationDashboardQuery(requesterId, isAdmin, studentId), ct);
        return Ok(result);
    }

    #endregion
}
