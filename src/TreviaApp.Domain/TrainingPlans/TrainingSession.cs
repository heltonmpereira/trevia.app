using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.TrainingPlans;

public class TrainingSession : Entity
{
    public Guid TrainingPlanId { get; private set; }
    public TrainingPlan TrainingPlan { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public string? Description { get; private set; }
    public DayOfWeek? SuggestedDayOfWeek { get; private set; }
    public TimeSpan? EstimatedDurationMin { get; private set; }
    public string? CoachNotesInternal { get; private set; }
    public string? Focus { get; private set; }

    private readonly List<SessionExercise> _exercises = new();
    public IReadOnlyCollection<SessionExercise> Exercises => _exercises.AsReadOnly();

    private TrainingSession() { }

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

    public void SetOrder(int newOrder)
    {
        if (newOrder < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(newOrder));
        Order = newOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

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

    public void RemoveExercise(Guid sessionExerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == sessionExerciseId);
        if (exercise == null) return;
        _exercises.Remove(exercise);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

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

    public SessionExercise? FindSessionExercise(Guid sessionExerciseId)
        => _exercises.FirstOrDefault(e => e.Id == sessionExerciseId);

    internal void ImportExercises(List<SessionExercise> copiedExercises)
    {
        _exercises.AddRange(copiedExercises);
    }
}
