using Microsoft.Extensions.Hosting;

namespace TreviaApp.Client.Services;

public class SyncBackgroundService : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly PeriodicTimer _timer;
    private bool _previousOnline = true;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public SyncBackgroundService(IServiceProvider services)
    {
        _services = services;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = ExecuteAsync(_cts.Token);
        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _cts?.Cancel();
        }
        finally
        {
            await _executingTask
                .WaitAsync(cancellationToken)
                .ContinueWith(_ => { }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
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

    public void Dispose()
    {
        _timer.Dispose();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
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
