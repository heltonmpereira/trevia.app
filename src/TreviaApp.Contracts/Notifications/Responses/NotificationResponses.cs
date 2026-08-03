using TreviaApp.Contracts.Common;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Notifications.Responses;

public sealed record NotificationResponse
{
    public NotificationResponse() { }

    public NotificationResponse(
        Guid id,
        NotificationType type,
        string title,
        string message,
        NotificationReferenceType? referenceType,
        Guid? referenceId,
        bool isRead,
        DateTimeOffset createdAt,
        DateTimeOffset? readAt)
    {
        Id = id;
        Type = type;
        Title = title;
        Message = message;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        IsRead = isRead;
        CreatedAt = createdAt;
        ReadAt = readAt;
    }

    public Guid Id { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationReferenceType? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}

public sealed record UnreadCountResponse
{
    public UnreadCountResponse() { }

    public UnreadCountResponse(int unreadCount, DateTimeOffset? lastNotificationAt)
    {
        UnreadCount = unreadCount;
        LastNotificationAt = lastNotificationAt;
    }

    public int UnreadCount { get; init; }
    public DateTimeOffset? LastNotificationAt { get; init; }
}

public sealed record MarkManyResultResponse
{
    public MarkManyResultResponse() { }

    public MarkManyResultResponse(int affectedCount)
    {
        AffectedCount = affectedCount;
    }

    public int AffectedCount { get; init; }
}
