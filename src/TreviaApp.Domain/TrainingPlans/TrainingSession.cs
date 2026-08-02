using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.TrainingPlans;

/// <summary>
/// Represents the TrainingSession domain entity.
/// </summary>
public class TrainingSession : Entity
{
    /// <summary>
    /// Gets Training Plan Id.
    /// </summary>
    public Guid TrainingPlanId { get; private set; }
    /// <summary>
    /// Gets Training Plan.
    /// </summary>
    public TrainingPlan TrainingPlan { get; private set; } = null!;

    /// <summary>
    /// Gets Name.
    /// </summary>
    public string Name { get; private set; } = null!;
    /// <summary>
    /// Gets Order.
    /// </summary>
    public int Order { get; private set; }
    /// <summary>
    /// Gets Description.
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// Gets Suggested Day Of Week.
    /// </summary>
    public DayOfWeek? SuggestedDayOfWeek { get; private set; }
    /// <summary>
    /// Gets Estimated Duration Min.
    /// </summary>
    public TimeSpan? EstimatedDurationMin { get; private set; }
    /// <summary>
    /// Gets Coach Notes Internal.
    /// </summary>
    public string? CoachNotesInternal { get; private set; }
    /// <summary>
    /// Gets Focus.
    /// </summary>
    public string? Focus { get; private set; }

    private readonly List<SessionExercise> _exercises = new();
    /// <summary>
    /// Gets Exercises.
    /// </summary>
    public IReadOnlyCollection<SessionExercise> Exercises => _exercises.AsReadOnly();

    private TrainingSession() { }

    /// <summary>
    /// Initializes a new instance of the TrainingSession class.
    /// </summary>
    public TrainingSession(
        Guid trainingPlanId,
        string name,
        int order,
        string? description = null,
        DayOfWeek? suggestedDayOfWeek = null,
        TimeSpan? estimatedDurationMin = null,
        string? coachNotesInternal = null,
        string? focus = null)
    {
        if (trainingPlanId == Guid.Empty)
            throw new ArgumentException("TrainingPlanId cannot be empty.", nameof(trainingPlanId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name too long (> 100).", nameof(name));
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(order));
        if (description != null && description.Length > 500)
            throw new ArgumentException("Description too long (> 500).", nameof(description));
        if (coachNotesInternal != null && coachNotesInternal.Length > 2000)
            throw new ArgumentException("CoachNotesInternal too long (> 2000).", nameof(coachNotesInternal));
        if (focus != null && focus.Length > 500)
            throw new ArgumentException("Focus too long (> 500).", nameof(focus));

        TrainingPlanId = trainingPlanId;
        Name = name;
        Order = order;
        Description = description;
        SuggestedDayOfWeek = suggestedDayOfWeek;
        EstimatedDurationMin = estimatedDurationMin;
        CoachNotesInternal = coachNotesInternal;
        Focus = focus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Update.
    /// </summary>
    public void Update(
        string name,
        int order,
        string? description,
        DayOfWeek? suggestedDayOfWeek,
        TimeSpan? estimatedDurationMin,
        string? coachNotesInternal,
        string? focus)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name too long (> 100).", nameof(name));
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(order));
        if (description != null && description.Length > 500)
            throw new ArgumentException("Description too long (> 500).", nameof(description));
        if (coachNotesInternal != null && coachNotesInternal.Length > 2000)
            throw new ArgumentException("CoachNotesInternal too long (> 2000).", nameof(coachNotesInternal));
        if (focus != null && focus.Length > 500)
            throw new ArgumentException("Focus too long (> 500).", nameof(focus));

        Name = name;
        Order = order;
        Description = description;
        SuggestedDayOfWeek = suggestedDayOfWeek;
        EstimatedDurationMin = estimatedDurationMin;
        CoachNotesInternal = coachNotesInternal;
        Focus = focus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Set Order.
    /// </summary>
    public void SetOrder(int newOrder)
    {
        if (newOrder < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(newOrder));
        Order = newOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Exercise.
    /// </summary>
    public Guid AddExercise(
        Guid exerciseId,
        int order,
        string? notesForStudent = null,
        string? notesForCoach = null)
    {
        if (_exercises.Any(e => e.Order == order))
            throw new InvalidOperationException($"Exercise with order {order} already exists in this session.");

        var sessionExercise = new SessionExercise(
            Id,
            exerciseId,
            order,
            notesForStudent,
            notesForCoach);

        _exercises.Add(sessionExercise);
        UpdatedAt = DateTimeOffset.UtcNow;
        return sessionExercise.Id;
    }

    /// <summary>
    /// Executes Remove Exercise.
    /// </summary>
    public void RemoveExercise(Guid sessionExerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == sessionExerciseId);
        if (exercise == null) return;
        _exercises.Remove(exercise);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Reorder Exercises.
    /// </summary>
    public void ReorderExercises(Dictionary<Guid, int> orders)
    {
        if (orders == null)
            throw new ArgumentNullException(nameof(orders));

        var usedNumbers = new HashSet<int>();
        foreach (var kvp in orders)
        {
            if (!usedNumbers.Add(kvp.Value))
                throw new InvalidOperationException($"Duplicate order number {kvp.Value} in reorder.");

            var exercise = _exercises.FirstOrDefault(e => e.Id == kvp.Key);
            if (exercise != null)
                exercise.SetOrder(kvp.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Find Session Exercise.
    /// </summary>
    public SessionExercise? FindSessionExercise(Guid sessionExerciseId)
        => _exercises.FirstOrDefault(e => e.Id == sessionExerciseId);

    internal void ImportExercises(List<SessionExercise> copiedExercises)
    {
        _exercises.AddRange(copiedExercises);
    }
}
