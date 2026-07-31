namespace TreviaApp.Contracts.Common;

public class AuditEntry
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? ByUserId { get; set; }
    public DateTimeOffset At { get; set; }
    public IDictionary<string, object> Changes { get; set; } = new Dictionary<string, object>();
}
