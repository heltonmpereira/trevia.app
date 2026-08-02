namespace TreviaApp.Contracts.Common;

/// <summary>
/// Represents the AuditEntry contract.
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Gets or sets Entity Id.
    /// </summary>
    public Guid EntityId { get; set; }
    /// <summary>
    /// Gets or sets Entity Type.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Action.
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets By User Id.
    /// </summary>
    public Guid? ByUserId { get; set; }
    /// <summary>
    /// Gets or sets At.
    /// </summary>
    public DateTimeOffset At { get; set; }
    /// <summary>
    /// Gets or sets Changes.
    /// </summary>
    public IDictionary<string, object> Changes { get; set; } = new Dictionary<string, object>();
}
