namespace TreviaApp.Domain.Abstractions;

/// <summary>
/// Represents the AggregateRoot domain entity.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets Id.
    /// </summary>
    public Guid Id { get; protected set; }
    /// <summary>
    /// Gets Created At.
    /// </summary>
    public DateTimeOffset CreatedAt { get; protected set; }
    /// <summary>
    /// Gets Updated At.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; protected set; }
    /// <summary>
    /// Gets Is Deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Initializes a new aggregate root with a generated identifier and creation timestamp.
    /// </summary>
    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Delete.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Restore.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Get Domain Events.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// Executes Clear Domain Events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Registers a domain event in the aggregate root.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
