namespace TreviaApp.Domain.Abstractions;

/// <summary>
/// Represents the Entity domain entity.
/// </summary>
public abstract class Entity
{
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
    /// Initializes a new entity with a generated identifier and creation timestamp.
    /// </summary>
    protected Entity()
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
}
