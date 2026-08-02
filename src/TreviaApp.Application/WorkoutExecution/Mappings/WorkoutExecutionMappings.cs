using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Profiles;

namespace TreviaApp.Application.WorkoutExecution.Mappings;

public static class WorkoutExecutionMappings
{
    public static (long? seconds, long? activeSeconds, int excCount, int completedSets, decimal? totalVolume)
        AggregateWorkoutSessionTotals(this TreviaApp.Domain.WorkoutExecution.WorkoutSession ws)
    {
        long? seconds = ws.TotalDurationElapsed.HasValue ? (long?)ws.TotalDurationElapsed.Value.TotalSeconds : null;
        long? activeSeconds = ws.ActiveTime.HasValue ? (long?)ws.ActiveTime.Value.TotalSeconds : null;
        int excCount = ws.Exercises.Count;
        int completedSets = ws.Exercises.SelectMany(e => e.Sets).Count(s => s.Completed);
        var volumes = ws.Exercises.SelectMany(e => e.Sets).Where(s => s.VolumeKg.HasValue).Select(s => s.VolumeKg!.Value);
        decimal? totalVolume = volumes.Any() ? volumes.Sum() : null;
        return (seconds, activeSeconds, excCount, completedSets, totalVolume);
    }

    public static string? ExtractPhotoFileId(this UserProfile? profile)
        => profile != null && profile.Photo != null ? profile.Photo.FileId : null;
}
