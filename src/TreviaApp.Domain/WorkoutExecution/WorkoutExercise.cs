using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.WorkoutExecution;

public class WorkoutExercise : Entity
{
    public Guid WorkoutSessionId { get; private set; }
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    public Guid? SessionExerciseId { get; private set; }
    public SessionExercise? SessionExercise { get; private set; }

    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public int Order { get; private set; }

    public bool IsSkipped { get; private set; }
    public string? SkipReason { get; private set; }

    public string? Notes { get; private set; }

    private readonly List<WorkoutSet> _sets = new();
    public IReadOnlyCollection<WorkoutSet> Sets => _sets.AsReadOnly();

    private WorkoutExercise() { }

    public WorkoutExercise(
        Guid workoutSessionId,
        Guid? sessionExerciseId,
        Guid exerciseId,
        int order,
        string? notes = null)
    {
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("WorkoutSessionId cannot be empty.", nameof(workoutSessionId));
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (order < 1) throw new ArgumentOutOfRangeException(nameof(order));
        if (notes != null && notes.Length > 1000) throw new ArgumentException("Notes too long (> 1000).", nameof(notes));

        WorkoutSessionId = workoutSessionId;
        SessionExerciseId = sessionExerciseId;
        ExerciseId = exerciseId;
        Order = order;
        Notes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Skip(string? skipReason = null)
    {
        if (skipReason != null && skipReason.Length > 500)
            throw new ArgumentException("SkipReason too long (> 500).", nameof(skipReason));
        IsSkipped = true;
        SkipReason = skipReason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetNotes(string? notes)
    {
        if (notes != null && notes.Length > 1000)
            throw new ArgumentException("Notes too long (> 1000).", nameof(notes));
        Notes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddSetFromPrescription(
        Guid setPrescriptionId,
        int setNumber,
        int? targetRepsMin,
        int? targetRepsMax,
        decimal? targetLoadValue,
        PrescriptionLoadUnit targetLoadUnit,
        TimeSpan? targetRestSeconds,
        SetTechnique? technique,
        bool isAdditional = false)
    {
        if (_sets.Any(s => s.SetNumber == setNumber && !s.IsAdditionalSet))
            throw new InvalidOperationException($"Set number {setNumber} already exists.");

        var set = new WorkoutSet(
            Id,
            setPrescriptionId,
            setNumber,
            targetRepsMin,
            targetRepsMax,
            targetLoadValue,
            targetLoadUnit,
            targetRestSeconds,
            technique,
            isAdditional);

        _sets.Add(set);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public WorkoutSet AddExtraSet(int setNumber)
    {
        var realSetNumber = setNumber > 0 ? setNumber : (_sets.Count == 0 ? 1 : _sets.Max(s => s.SetNumber) + 1);
        var set = new WorkoutSet(
            workoutExerciseId: Id,
            setPrescriptionId: null,
            setNumber: realSetNumber,
            targetRepsMin: null,
            targetRepsMax: null,
            targetLoadValue: null,
            targetLoadUnit: PrescriptionLoadUnit.Kilograms,
            targetRestSeconds: null,
            techniqueApplied: SetTechnique.Standard,
            isAdditionalSet: true);

        _sets.Add(set);
        UpdatedAt = DateTimeOffset.UtcNow;
        return set;
    }

    public WorkoutSet? FindSet(Guid workoutSetId)
        => _sets.FirstOrDefault(s => s.Id == workoutSetId);
}
