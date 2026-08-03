using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Notifications.Responses;

namespace TreviaApp.Client.Services.Notifications;

public interface INotificationsService
{
    Task<UnreadCountResponse> GetUnreadCount(CancellationToken ct = default);
    Task<PaginatedResponse<NotificationResponse>> GetMyNotifications(int page = 1, int pageSize = 50, bool onlyUnread = false, CancellationToken ct = default);
    Task<NotificationResponse> GetNotificationById(Guid id, CancellationToken ct = default);
    Task<NotificationResponse> MarkAsRead(Guid id, CancellationToken ct = default);
    Task<MarkManyResultResponse> MarkAllAsRead(CancellationToken ct = default);
    Task DeleteNotification(Guid id, CancellationToken ct = default);
}
