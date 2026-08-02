using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.WorkoutExecution;

/// <summary>
/// Represents the WorkoutSession domain entity.
/// </summary>
public class WorkoutSession : AggregateRoot
{
    /// <summary>
    /// Gets Student Id.
    /// </summary>
    public Guid StudentId { get; private set; }

    /// <summary>
    /// Gets Student.
    /// </summary>
    public AppUser Student { get; private set; } = null!;

    /// <summary>
    /// Gets Training Plan Id.
    /// </summary>
    public Guid? TrainingPlanId { get; private set; }

    /// <summary>
    /// Gets Training Plan.
    /// </summary>
    public TrainingPlan? TrainingPlan { get; private set; }

    /// <summary>
    /// Gets Training Session Id.
    /// </summary>
    public Guid? TrainingSessionId { get; private set; }

    /// <summary>
    /// Gets Training Session.
    /// </summary>
    public TrainingSession? TrainingSession { get; private set; }

    /// <summary>
    /// Gets Name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets Status.
    /// </summary>
    public WorkoutStatus Status { get; private set; } = WorkoutStatus.NotStarted;

    /// <summary>
    /// Gets Started At.
    /// </summary>
    public DateTimeOffset? StartedAt { get; private set; }

    /// <summary>
    /// Gets Finished At.
    /// </summary>
    public DateTimeOffset? FinishedAt { get; private set; }

    /// <summary>
    /// Gets Total Duration Elapsed.
    /// </summary>
    public TimeSpan? TotalDurationElapsed =>
        StartedAt.HasValue
            ? (FinishedAt ?? DateTimeOffset.UtcNow) - StartedAt.Value
            : null;

    /// <summary>
    /// Gets Active Time.
    /// </summary>
    public TimeSpan? ActiveTime { get; private set; }

    /// <summary>
    /// Gets Calories Burned.
    /// </summary>
    public int? CaloriesBurned { get; private set; }

    /// <summary>
    /// Gets Overall Rating.
    /// </summary>
    public WorkoutRating? OverallRating { get; private set; }

    /// <summary>
    /// Gets General Notes.
    /// </summary>
    public string? GeneralNotes { get; private set; }

    /// <summary>
    /// Gets Week Number In Plan.
    /// </summary>
    public int WeekNumberInPlan { get; private set; } = 1;

    private readonly List<WorkoutExercise> _exercises = new();

    /// <summary>
    /// Gets Exercises.
    /// </summary>
    public IReadOnlyCollection<WorkoutExercise> Exercises => _exercises.AsReadOnly();

    private readonly List<WorkoutPause> _pauses = new();

    /// <summary>
    /// Gets Pauses.
    /// </summary>
    public IReadOnlyCollection<WorkoutPause> Pauses => _pauses.AsReadOnly();

    private WorkoutSession() { }

    /// <summary>
    /// Initializes a new instance of the WorkoutSession class.
    /// </summary>
    public WorkoutSession(
        Guid studentId,
        Guid? trainingPlanId,
        Guid? trainingSessionId,
        string name,
        int weekNumberInPlan = 1)
    {
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 200) throw new ArgumentException("Name too long (> 200).", nameof(name));
        if (weekNumberInPlan < 1) throw new ArgumentOutOfRangeException(nameof(weekNumberInPlan));

        StudentId = studentId;
        TrainingPlanId = trainingPlanId;
        TrainingSessionId = trainingSessionId;
        Name = name;
        WeekNumberInPlan = weekNumberInPlan;
        Status = WorkoutStatus.NotStarted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Start.
    /// </summary>
    public void Start()
    {
        if (Status == WorkoutStatus.Completed || Status == WorkoutStatus.Interrupted)
            throw new InvalidOperationException($"Cannot start session in status {Status}.");

        Status = WorkoutStatus.InProgress;
        StartedAt ??= DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Pause.
    /// </summary>
    public void Pause()
    {
        if (Status != WorkoutStatus.InProgress)
            throw new InvalidOperationException($"Can only pause when InProgress. Current: {Status}.");

        var pause = new WorkoutPause(Id, DateTimeOffset.UtcNow);
        _pauses.Add(pause);
        Status = WorkoutStatus.Paused;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Resume.
    /// </summary>
    public void Resume()
    {
        if (Status != WorkoutStatus.Paused)
            throw new InvalidOperationException($"Can only resume when Paused. Current: {Status}.");

        var latestOpenPause = _pauses.LastOrDefault(p => !p.EndedAt.HasValue);
        latestOpenPause?.EndNow();

        Status = WorkoutStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Finish.
    /// </summary>
    public void Finish(WorkoutRating? overallRating = null, string? generalNotes = null, int? caloriesBurned = null)
    {
        if (Status != WorkoutStatus.InProgress && Status != WorkoutStatus.Paused)
            throw new InvalidOperationException($"Can only finish InProgress/Paused sessions. Current: {Status}.");

        if (generalNotes != null && generalNotes.Length > 2000)
            throw new ArgumentException("GeneralNotes too long (> 2000).", nameof(generalNotes));

        if (caloriesBurned < 0)
            throw new ArgumentOutOfRangeException(nameof(caloriesBurned));

        foreach (var pause in _pauses)
            pause.EnsureEnded();

        FinishedAt = DateTimeOffset.UtcNow;
        var pausedTotal = TimeSpan.FromSeconds(_pauses.Sum(p => p.Duration?.TotalSeconds ?? 0));
        var elapsed = FinishedAt.Value - StartedAt!.Value;
        ActiveTime = elapsed - pausedTotal;
        if (ActiveTime < TimeSpan.Zero) ActiveTime = TimeSpan.Zero;

        OverallRating = overallRating;
        GeneralNotes = generalNotes;
        CaloriesBurned = caloriesBurned;
        Status = overallRating == WorkoutRating.Interrupted
            ? WorkoutStatus.Interrupted
            : WorkoutStatus.Completed;

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Exercises From Prescription.
    /// </summary>
    public void AddExercisesFromPrescription(IEnumerable<(Guid sessionExerciseId, Guid exerciseId, int order, string? notesForStudent)> list)
    {
        if (Status != WorkoutStatus.NotStarted && Status != WorkoutStatus.InProgress)
            throw new InvalidOperationException($"Cannot add exercises at status {Status}.");

        foreach (var item in list)
        {
            var wex = new WorkoutExercise(
                Id,
                item.sessionExerciseId,
                item.exerciseId,
                item.order,
                item.notesForStudent);
            _exercises.Add(wex);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Find Exercise.
    /// </summary>
    public WorkoutExercise? FindExercise(Guid workoutExerciseId)
        => _exercises.FirstOrDefault(e => e.Id == workoutExerciseId);
}
