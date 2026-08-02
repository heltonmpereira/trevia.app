namespace TreviaApp.Contracts.Common;

/// <summary>
/// Represents the EntityResponse contract.
/// </summary>
public class EntityResponse
{
    /// <summary>
    /// Gets or sets Id.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets Created At.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets Updated At.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
