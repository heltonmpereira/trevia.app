namespace TreviaApp.Domain.Interfaces;

/// <summary>
/// Defines the IUnitOfWork contract.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes to the underlying data store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
