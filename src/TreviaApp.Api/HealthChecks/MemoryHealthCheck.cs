namespace TreviaApp.Api.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
    private readonly double _maxMemoryThresholdPercent;
    private readonly long _maxMemoryBytesAbsolute;

    public MemoryHealthCheck(double maxMemoryPercentThreshold = 0.8, long maxMemoryMBytesAbsolute = 4096)
    {
        _maxMemoryThresholdPercent = maxMemoryPercentThreshold;
        _maxMemoryBytesAbsolute = maxMemoryMBytesAbsolute * 1024 * 1024;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var processMemory = GC.GetTotalMemory(false);
            var memoryInfo = GC.GetGCMemoryInfo();
            long totalCommitted = memoryInfo.TotalCommittedBytes;
            double heapSizeMb = Math.Round(processMemory / 1024.0 / 1024.0, 2);
            double committedMb = Math.Round(totalCommitted / 1024.0 / 1024.0, 2);

            var data = new Dictionary<string, object>
            {
                ["gc_total_memory_mb"] = heapSizeMb,
                ["gc_committed_mb"] = committedMb,
                ["gc_collection_count_0"] = GC.CollectionCount(0),
                ["gc_collection_count_1"] = GC.CollectionCount(1),
                ["gc_collection_count_2"] = GC.CollectionCount(2)
            };

            if (processMemory > _maxMemoryBytesAbsolute)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Memory usage too high: {heapSizeMb} MB (limit {_maxMemoryBytesAbsolute / 1024 / 1024} MB)",
                    null, data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory usage healthy: {heapSizeMb} MB heap, {committedMb} MB committed", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(
                context.Registration.FailureStatus,
                "Failed to check memory health", ex));
        }
    }
}
