namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Notifications;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Notifications.Responses;
using TreviaApp.Shared.Constants;

[ApiController]
[Route("api/notifications")]
[Authorize]
[EnableRateLimiting("AuthEndpoint")]
[Produces("application/json")]
public class NotificationsController : ApiControllerBase
{
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetUnreadCountQuery(userId), ct);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool onlyUnread = false,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetMyNotificationsQuery(userId, page, pageSize, onlyUnread), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new GetNotificationByIdQuery(userId, id), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new MarkNotificationReadCommand(userId, id), ct);
        return Ok(result);
    }

    [HttpPut("read-all")]
    [ProducesResponseType(typeof(MarkManyResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        var result = await Sender.Send(new MarkAllNotificationsReadCommand(userId), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var userId = CurrentUser.UserId!.Value;
        await Sender.Send(new DeleteNotificationCommand(userId, id), ct);
        return NoContent();
    }
}
