using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.WorkoutExecution;

public class WorkoutPause : Entity
{
    public Guid WorkoutSessionId { get; private set; }
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    public TimeSpan? Duration => EndedAt.HasValue ? EndedAt.Value - StartedAt : null;

    private WorkoutPause() { }

    public WorkoutPause(Guid workoutSessionId, DateTimeOffset startedAt)
    {
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("WorkoutSessionId cannot be empty.", nameof(workoutSessionId));
        if (startedAt == default) throw new ArgumentException("StartedAt cannot be default.", nameof(startedAt));

        WorkoutSessionId = workoutSessionId;
        StartedAt = startedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EndNow()
    {
        EndedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EnsureEnded()
    {
        if (!EndedAt.HasValue) EndNow();
    }
}
