namespace TreviaApp.Api.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

public class DiskStorageHealthCheck : IHealthCheck
{
    private readonly string _rootPath;
    private readonly long _minFreeBytes;

    public DiskStorageHealthCheck(string? rootPath = null, long minFreeMBytes = 100)
    {
        _rootPath = rootPath ?? AppContext.BaseDirectory;
        _minFreeBytes = minFreeMBytes * 1024 * 1024;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            DriveInfo drive;
            try
            {
                drive = new DriveInfo(_rootPath);
            }
            catch
            {
                drive = DriveInfo.GetDrives().FirstOrDefault(d => d.DriveType == DriveType.Fixed)
                        ?? DriveInfo.GetDrives().First();
            }

            if (!drive.IsReady)
            {
                return Task.FromResult(HealthCheckResult.Degraded($"Drive {drive.Name} not ready"));
            }

            long free = drive.AvailableFreeSpace;
            long total = drive.TotalSize;
            var data = new Dictionary<string, object>
            {
                ["drive"] = drive.Name,
                ["free_bytes"] = free,
                ["free_mb"] = Math.Round(free / 1024.0 / 1024.0, 2),
                ["total_bytes"] = total,
                ["used_percent"] = Math.Round(100.0 * (total - free) / total, 2)
            };

            if (free >= _minFreeBytes)
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Disk has {data["free_mb"]} MB available", data));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Disk critically low: {data["free_mb"]} MB free (required {_minFreeBytes / 1024 / 1024} MB)",
                null, data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(
                context.Registration.FailureStatus,
                "Failed to check disk storage", ex));
        }
    }
}
