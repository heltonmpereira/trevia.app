using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Notifications.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Notifications;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Application.Notifications;

#region ====================  COMMANDS  ====================

public sealed record MarkNotificationReadCommand(
    Guid CurrentUserId,
    Guid NotificationId)
    : ICommand<NotificationResponse>;

public sealed record MarkAllNotificationsReadCommand(Guid CurrentUserId)
    : ICommand<MarkManyResultResponse>;

public sealed record DeleteNotificationCommand(
    Guid CurrentUserId,
    Guid NotificationId)
    : ICommand<bool>;

#endregion

#region ====================  QUERIES  ====================

public sealed record GetMyNotificationsQuery(
    Guid CurrentUserId,
    int Page = 1,
    int PageSize = 50,
    bool OnlyUnread = false)
    : IQuery<PaginatedResponse<NotificationResponse>>;

public sealed record GetNotificationByIdQuery(
    Guid CurrentUserId,
    Guid NotificationId)
    : IQuery<NotificationResponse>;

public sealed record GetUnreadCountQuery(Guid CurrentUserId)
    : IQuery<UnreadCountResponse>;

#endregion

#region ====================  VALIDATORS  ====================

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(c => c.NotificationId).NotEmpty();
    }
}

public sealed class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(c => c.NotificationId).NotEmpty();
    }
}

#endregion

#region ====================  HANDLERS (Commands)  ====================

public sealed class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, NotificationResponse>
{
    private readonly IApplicationDbContext _db;
    public MarkNotificationReadCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<NotificationResponse> Handle(MarkNotificationReadCommand c, CancellationToken ct)
    {
        var n = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == c.NotificationId, ct);
        if (n == null)
            throw new DomainException("Notificação não encontrada.", ErrorCodes.NotificationNotFound);
        n.MarkAsRead(c.CurrentUserId);
        await _db.SaveChangesAsync(ct);
        return new NotificationResponse(
            n.Id, n.Type, n.Title, n.Message, n.ReferenceType, n.ReferenceId,
            n.IsRead, n.CreatedAt, n.ReadAt);
    }
}

public sealed class MarkAllNotificationsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsReadCommand, MarkManyResultResponse>
{
    private readonly IApplicationDbContext _db;
    public MarkAllNotificationsReadCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<MarkManyResultResponse> Handle(MarkAllNotificationsReadCommand c, CancellationToken ct)
    {
        var notifs = await _db.Set<Notification>()
            .Where(n => n.UserId == c.CurrentUserId && !n.IsRead)
            .ToListAsync(ct);
        foreach (var n in notifs)
            n.MarkAllAsReadBypass();
        await _db.SaveChangesAsync(ct);
        return new MarkManyResultResponse(notifs.Count);
    }
}

public sealed class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteNotificationCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<bool> Handle(DeleteNotificationCommand c, CancellationToken ct)
    {
        var n = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == c.NotificationId, ct);
        if (n == null)
            throw new DomainException("Notificação não encontrada.", ErrorCodes.NotificationNotFound);
        if (n.UserId != c.CurrentUserId)
            throw new DomainException("Esta notificação não pertence a você.", ErrorCodes.NotificationNotOwner);
        n.Delete();
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

#endregion

#region ====================  HANDLERS (Queries)  ====================

public sealed class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, PaginatedResponse<NotificationResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetMyNotificationsQueryHandler(IApplicationDbContext db) { _db = db; }

    public async Task<PaginatedResponse<NotificationResponse>> Handle(GetMyNotificationsQuery q, CancellationToken ct)
    {
        var baseQuery = _db.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.UserId == q.CurrentUserId);

        if (q.OnlyUnread)
            baseQuery = baseQuery.Where(n => !n.IsRead);

        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderByDescending(n => n.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(n => new NotificationResponse(
                n.Id, n.Type, n.Title, n.Message, n.ReferenceType, n.ReferenceId,
                n.IsRead, n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);

        return new PaginatedResponse<NotificationResponse>
        {
            Items = items,
            TotalCount = total,
            PageIndex = q.Page,
            PageSize = q.PageSize,
            HasNextPage = (q.Page * q.PageSize) < total
        };
    }
}

public sealed class GetNotificationByIdQueryHandler
    : IRequestHandler<GetNotificationByIdQuery, NotificationResponse>
{
    private readonly IApplicationDbContext _db;
    public GetNotificationByIdQueryHandler(IApplicationDbContext db) { _db = db; }

    public async Task<NotificationResponse> Handle(GetNotificationByIdQuery q, CancellationToken ct)
    {
        var n = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == q.NotificationId, ct);
        if (n == null)
            throw new DomainException("Notificação não encontrada.", ErrorCodes.NotificationNotFound);
        if (n.UserId != q.CurrentUserId)
            throw new DomainException("Esta notificação não pertence a você.", ErrorCodes.NotificationNotOwner);

        // Automatically mark as read on detail view
        n.MarkAsRead(q.CurrentUserId);
        await _db.SaveChangesAsync(ct);

        return new NotificationResponse(
            n.Id, n.Type, n.Title, n.Message, n.ReferenceType, n.ReferenceId,
            n.IsRead, n.CreatedAt, n.ReadAt);
    }
}

public sealed class GetUnreadCountQueryHandler
    : IRequestHandler<GetUnreadCountQuery, UnreadCountResponse>
{
    private readonly IApplicationDbContext _db;
    public GetUnreadCountQueryHandler(IApplicationDbContext db) { _db = db; }

    public async Task<UnreadCountResponse> Handle(GetUnreadCountQuery q, CancellationToken ct)
    {
        var count = await _db.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.UserId == q.CurrentUserId && !n.IsRead)
            .CountAsync(ct);

        var last = await _db.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.UserId == q.CurrentUserId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => (DateTimeOffset?)n.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return new UnreadCountResponse(count, last);
    }
}

#endregion
