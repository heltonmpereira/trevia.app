namespace TreviaApp.Client.Services;

public enum SyncItemStatus { Pending, Processing, Completed, Failed }

public interface ISyncQueue
{
    event Action? OnStatusChanged;
    Task EnqueueAsync<T>(string userId, string operationType, T payload, Guid? clientRequestId = null);
    Task<IReadOnlyList<SyncQueueItem>> GetPendingItemsAsync(string userId);
    Task<SyncStatusSummary> GetStatusSummaryAsync(string userId);
    Task UpdateStatusAsync(Guid id, SyncItemStatus status, string? lastError = null, int? retryCount = null);
    Task ClearCompletedAsync(string userId);
    Task<bool> ProcessPendingAsync(string userId, Func<SyncQueueItem, Task<bool>> processor);
}

public class SyncQueueItem
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public string? PayloadJson { get; set; }
    public SyncItemStatus Status { get; set; } = SyncItemStatus.Pending;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SyncStatusSummary
{
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Failed { get; set; }
    public int Completed { get; set; }
    public bool IsOnline { get; set; }
    public IReadOnlyList<SyncFailedItemInfo> FailedItems { get; set; } = Array.Empty<SyncFailedItemInfo>();
}

public class SyncFailedItemInfo
{
    public Guid Id { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string? LastError { get; set; }
}
