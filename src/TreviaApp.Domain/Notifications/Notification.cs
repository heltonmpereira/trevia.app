using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Notifications;

/// <summary>
/// Notificação interna persistida em banco para um usuário (Aluno/Professor).
/// US-1005 / US-1007: Lista, badge de não lidas, marcação de leitura e exclusão lógica.
/// </summary>
public class Notification : AggregateRoot
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    public NotificationReferenceType? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }

    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    public Notification(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        NotificationReferenceType? referenceType = null,
        Guid? referenceId = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Notification title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Notification message cannot be empty.", nameof(message));
        if (title.Length > 200) throw new ArgumentException("Notification title cannot exceed 200 characters.", nameof(title));
        if (message.Length > 1000) throw new ArgumentException("Notification message cannot exceed 1000 characters.", nameof(message));
        if (referenceType.HasValue != referenceId.HasValue)
            throw new ArgumentException("ReferenceType and ReferenceId must be provided together or both null.");

        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        IsRead = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRead(Guid readerUserId)
    {
        if (IsRead) return;
        if (readerUserId != UserId) throw new UnauthorizedAccessException("Only the notification owner can mark it as read.");
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAllAsReadBypass()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
