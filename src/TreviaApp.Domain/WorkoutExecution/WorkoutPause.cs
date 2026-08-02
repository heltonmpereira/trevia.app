using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.WorkoutExecution;

/// <summary>
/// Represents the WorkoutPause domain entity.
/// </summary>
public class WorkoutPause : Entity
{
    /// <summary>
    /// Gets Workout Session Id.
    /// </summary>
    public Guid WorkoutSessionId { get; private set; }

    /// <summary>
    /// Gets Workout Session.
    /// </summary>
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    /// <summary>
    /// Gets Started At.
    /// </summary>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>
    /// Gets Ended At.
    /// </summary>
    public DateTimeOffset? EndedAt { get; private set; }

    /// <summary>
    /// Gets Duration.
    /// </summary>
    public TimeSpan? Duration => EndedAt.HasValue ? EndedAt.Value - StartedAt : null;

    private WorkoutPause() { }

    /// <summary>
    /// Initializes a new instance of the WorkoutPause class.
    /// </summary>
    public WorkoutPause(Guid workoutSessionId, DateTimeOffset startedAt)
    {
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("WorkoutSessionId cannot be empty.", nameof(workoutSessionId));
        if (startedAt == default) throw new ArgumentException("StartedAt cannot be default.", nameof(startedAt));

        WorkoutSessionId = workoutSessionId;
        StartedAt = startedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes End Now.
    /// </summary>
    public void EndNow()
    {
        EndedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Ensure Ended.
    /// </summary>
    public void EnsureEnded()
    {
        if (!EndedAt.HasValue) EndNow();
    }
}
