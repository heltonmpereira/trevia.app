namespace TreviaApp.Domain.Identity;

public class ProcessedClientRequest
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string? ResponsePayload { get; set; }
    public int StatusCode { get; set; }
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;

    public AppUser? User { get; set; }
}
