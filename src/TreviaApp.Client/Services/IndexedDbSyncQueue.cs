using Microsoft.JSInterop;
using System.Text.Json;

namespace TreviaApp.Client.Services;

public class IndexedDbSyncQueue : ISyncQueue, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public event Action? OnStatusChanged;

    public IndexedDbSyncQueue(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/idb-storage.js").AsTask());
    }

    private void NotifyChanged() => OnStatusChanged?.Invoke();

    public async Task EnqueueAsync<T>(string userId, string operationType, T payload, Guid? clientRequestId = null)
    {
        var item = new
        {
            id = clientRequestId ?? Guid.NewGuid(),
            userId,
            operationType,
            payloadJson = JsonSerializer.Serialize(payload),
            payload = (object?)payload
        };
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("enqueueSync", item);
        }
        catch
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem",
                    $"treviaapp_sync_{item.id}",
                    JsonSerializer.Serialize(new SyncQueueItem
                    {
                        Id = item.id,
                        UserId = userId,
                        OperationType = operationType,
                        Status = SyncItemStatus.Pending,
                        PayloadJson = item.payloadJson,
                        CreatedAt = DateTimeOffset.UtcNow
                    }));
            }
            catch (Exception innerEx)
            {
                throw new InvalidOperationException("Falha ao enfileirar operação para sincronização.", innerEx);
            }
        }
        NotifyChanged();
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetPendingItemsAsync(string userId)
    {
        try
        {
            var module = await _moduleTask.Value;
            var raw = await module.InvokeAsync<JsonElement[]>("getPendingSync", userId);
            return raw.Select(r => MapToItem(r)).ToList().AsReadOnly();
        }
        catch
        {
            return new List<SyncQueueItem>().AsReadOnly();
        }
    }

    public async Task<SyncStatusSummary> GetStatusSummaryAsync(string userId)
    {
        bool isOnline = true;
        try
        {
            var module = await _moduleTask.Value;
            isOnline = await module.InvokeAsync<bool>("isOnline");
            var raw = await module.InvokeAsync<JsonElement>("getAllSyncStatus", userId);
            return new SyncStatusSummary
            {
                Pending = raw.GetProperty("pending").GetInt32(),
                Processing = raw.GetProperty("processing").GetInt32(),
                Failed = raw.GetProperty("failed").GetInt32(),
                Completed = raw.GetProperty("completed").GetInt32(),
                IsOnline = isOnline,
                FailedItems = raw.TryGetProperty("failedItems", out var failedItems)
                    ? failedItems.EnumerateArray().Select(f => new SyncFailedItemInfo
                    {
                        Id = f.GetProperty("id").GetGuid(),
                        OperationType = f.GetProperty("operationType").GetString() ?? "",
                        LastError = f.TryGetProperty("lastError", out var err) ? err.GetString() : null
                    }).ToList().AsReadOnly()
                    : Array.Empty<SyncFailedItemInfo>()
            };
        }
        catch
        {
            return new SyncStatusSummary { IsOnline = isOnline };
        }
    }

    public async Task UpdateStatusAsync(Guid id, SyncItemStatus status, string? lastError = null, int? retryCount = null)
    {
        try
        {
            var module = await _moduleTask.Value;
            var changes = new Dictionary<string, object?>
            {
                ["status"] = status.ToString().ToLowerInvariant()
            };
            if (lastError != null) changes["lastError"] = lastError;
            if (retryCount.HasValue) changes["retryCount"] = retryCount.Value;
            await module.InvokeVoidAsync("updateSyncItem", id, changes);
        }
        catch { }
        NotifyChanged();
    }

    public async Task ClearCompletedAsync(string userId)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("clearCompletedSync", userId);
        }
        catch { }
        NotifyChanged();
    }

    public async Task<bool> ProcessPendingAsync(string userId, Func<SyncQueueItem, Task<bool>> processor)
    {
        var items = await GetPendingItemsAsync(userId);
        if (!items.Any()) return true;

        bool allSucceeded = true;
        foreach (var item in items)
        {
            try
            {
                await UpdateStatusAsync(item.Id, SyncItemStatus.Processing);
                bool success = await processor(item);
                if (success)
                {
                    await UpdateStatusAsync(item.Id, SyncItemStatus.Completed, retryCount: item.RetryCount);
                }
                else
                {
                    allSucceeded = false;
                    await UpdateStatusAsync(item.Id, SyncItemStatus.Failed, "Processor returned false", item.RetryCount + 1);
                }
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                await UpdateStatusAsync(item.Id, SyncItemStatus.Failed, ex.Message, item.RetryCount + 1);
            }
        }
        NotifyChanged();
        return allSucceeded;
    }

    private static SyncQueueItem MapToItem(JsonElement r)
    {
        var item = new SyncQueueItem
        {
            Id = r.GetProperty("id").GetGuid(),
            UserId = r.GetProperty("userId").GetString() ?? "",
            OperationType = r.GetProperty("operationType").GetString() ?? "",
            RetryCount = r.TryGetProperty("retryCount", out var rc) ? rc.GetInt32() : 0,
            LastError = r.TryGetProperty("lastError", out var err) ? err.GetString() : null,
            CreatedAt = r.TryGetProperty("createdAt", out var ca) && ca.TryGetDateTimeOffset(out var dto) ? dto : DateTimeOffset.UtcNow
        };
        if (r.TryGetProperty("payloadJson", out var pj))
        {
            item.PayloadJson = pj.GetString();
        }
        if (r.TryGetProperty("status", out var statusProp))
        {
            var statusStr = statusProp.GetString()?.ToLowerInvariant();
            item.Status = statusStr switch
            {
                "pending" => SyncItemStatus.Pending,
                "processing" => SyncItemStatus.Processing,
                "completed" => SyncItemStatus.Completed,
                "failed" => SyncItemStatus.Failed,
                _ => SyncItemStatus.Pending
            };
        }
        return item;
    }

    [JSInvokable]
    public void JsOnlineChanged(bool isOnline)
    {
        NotifyChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            try { await (await _moduleTask.Value).DisposeAsync(); }
            catch (JSException) { }
        }
    }
}
