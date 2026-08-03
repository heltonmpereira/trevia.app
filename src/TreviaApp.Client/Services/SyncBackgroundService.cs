namespace TreviaApp.Client.Services;

public class SyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly PeriodicTimer _timer;
    private bool _previousOnline = true;

    public SyncBackgroundService(IServiceProvider services)
    {
        _services = services;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var queue = scope.ServiceProvider.GetRequiredService<ISyncQueue>();
                var currentUserProvider = scope.ServiceProvider.GetService<ICurrentUserIdProvider>();
                if (currentUserProvider != null)
                {
                    var userId = await currentUserProvider.GetCurrentUserIdAsync();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var status = await queue.GetStatusSummaryAsync(userId);
                        if ((status.Pending > 0 || status.Failed > 0) && status.IsOnline)
                        {
                            try
                            {
                                await queue.ProcessPendingAsync(userId, ProcessSingleItemAsync);
                            }
                            catch { }
                        }
                        if (status.IsOnline != _previousOnline && status.IsOnline && (status.Pending > 0 || status.Failed > 0))
                        {
                            try { await queue.ProcessPendingAsync(userId, ProcessSingleItemAsync); }
                            catch { }
                        }
                        _previousOnline = status.IsOnline;
                    }
                }
            }
            catch
            {
            }

            try
            {
                await _timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<bool> ProcessSingleItemAsync(SyncQueueItem item)
    {
        await Task.Delay(5);
        return true;
    }

    public override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }
}

public interface ICurrentUserIdProvider
{
    Task<string?> GetCurrentUserIdAsync();
}

public class DefaultCurrentUserIdProvider : ICurrentUserIdProvider
{
    public Task<string?> GetCurrentUserIdAsync()
    {
        return Task.FromResult<string?>(null);
    }
}
