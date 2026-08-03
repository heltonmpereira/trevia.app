using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Gamification;

public class PointTransaction : Entity
{
    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public int Amount { get; private set; }

    public PointReason Reason { get; private set; }

    public string? ReferenceType { get; private set; }

    public Guid? ReferenceId { get; private set; }

    public string? Description { get; private set; }

    public new DateTimeOffset CreatedAt { get; private set; }

    private PointTransaction()
    {
    }

    public PointTransaction(
        Guid userId,
        int amount,
        PointReason reason,
        string? referenceType = null,
        Guid? referenceId = null,
        string? description = null)
    {
        UserId = userId;
        Amount = amount;
        Reason = reason;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
