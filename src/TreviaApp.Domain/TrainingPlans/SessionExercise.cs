using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.TrainingPlans;

public class SessionExercise : Entity
{
    public Guid TrainingSessionId { get; private set; }
    public TrainingSession TrainingSession { get; private set; } = null!;

    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public int Order { get; private set; }
    public string? NotesForStudent { get; private set; }
    public string? NotesForCoach { get; private set; }
    public TimeSpan? RestBetweenSetsSeconds { get; private set; } = TimeSpan.FromSeconds(90);
    public SetTechnique? GlobalSetTechniqueAppliedToAllSets { get; private set; }
    public decimal? GlobalLoadOverrideKg { get; private set; }
    public int? GlobalRepsOverride { get; private set; }

    private readonly List<SetPrescription> _prescriptions = new();
    public IReadOnlyCollection<SetPrescription> Prescriptions => _prescriptions.AsReadOnly();

    private SessionExercise() { }

    public SessionExercise(
        Guid trainingSessionId,
        Guid exerciseId,
        int order,
        string? notesForStudent = null,
        string? notesForCoach = null,
        TimeSpan? restBetweenSetsSeconds = null,
        SetTechnique? globalSetTechniqueAppliedToAllSets = null,
        decimal? globalLoadOverrideKg = null,
        int? globalRepsOverride = null)
    {
        if (trainingSessionId == Guid.Empty)
            throw new ArgumentException("TrainingSessionId cannot be empty.", nameof(trainingSessionId));
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(order));
        if (notesForStudent != null && notesForStudent.Length > 1000)
            throw new ArgumentException("NotesForStudent too long (> 1000).", nameof(notesForStudent));
        if (notesForCoach != null && notesForCoach.Length > 1000)
            throw new ArgumentException("NotesForCoach too long (> 1000).", nameof(notesForCoach));

        TrainingSessionId = trainingSessionId;
        ExerciseId = exerciseId;
        Order = order;
        NotesForStudent = notesForStudent;
        NotesForCoach = notesForCoach;
        RestBetweenSetsSeconds = restBetweenSetsSeconds ?? TimeSpan.FromSeconds(90);
        GlobalSetTechniqueAppliedToAllSets = globalSetTechniqueAppliedToAllSets;
        GlobalLoadOverrideKg = globalLoadOverrideKg;
        GlobalRepsOverride = globalRepsOverride;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateBasicInfo(
        int order,
        string? notesForStudent,
        string? notesForCoach,
        TimeSpan? restBetweenSetsSeconds,
        SetTechnique? globalSetTechniqueAppliedToAllSets,
        decimal? globalLoadOverrideKg,
        int? globalRepsOverride)
    {
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(order));
        if (notesForStudent != null && notesForStudent.Length > 1000)
            throw new ArgumentException("NotesForStudent too long (> 1000).", nameof(notesForStudent));
        if (notesForCoach != null && notesForCoach.Length > 1000)
            throw new ArgumentException("NotesForCoach too long (> 1000).", nameof(notesForCoach));

        Order = order;
        NotesForStudent = notesForStudent;
        NotesForCoach = notesForCoach;
        RestBetweenSetsSeconds = restBetweenSetsSeconds;
        GlobalSetTechniqueAppliedToAllSets = globalSetTechniqueAppliedToAllSets;
        GlobalLoadOverrideKg = globalLoadOverrideKg;
        GlobalRepsOverride = globalRepsOverride;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetOrder(int newOrder)
    {
        if (newOrder < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(newOrder));
        Order = newOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid AddPrescriptionSet(
        int setNumber,
        int? targetRepsMin = null,
        int? targetRepsMax = null,
        decimal? loadValue = null,
        PrescriptionLoadUnit loadUnit = PrescriptionLoadUnit.Kilograms,
        TimeSpan? restSeconds = null,
        SetTechnique? technique = null,
        int? tempoTutSeconds = null,
        string? notes = null)
    {
        if (_prescriptions.Any(p => p.SetNumber == setNumber))
            throw new InvalidOperationException($"Set number {setNumber} already exists in this exercise.");

        var techniqueToUse = technique ?? GlobalSetTechniqueAppliedToAllSets ?? SetTechnique.Standard;
        TimeSpan? tut = tempoTutSeconds.HasValue ? TimeSpan.FromSeconds(tempoTutSeconds.Value) : null;

        var set = new SetPrescription(
            Id,
            setNumber,
            targetRepsMin,
            targetRepsMax,
            loadValue,
            loadUnit,
            restSeconds,
            techniqueToUse,
            notesProfessor: notes,
            tempoUnderTensionTUTSeconds: tut);

        _prescriptions.Add(set);
        UpdatedAt = DateTimeOffset.UtcNow;
        return set.Id;
    }

    public void UpdatePrescriptionSet(
        Guid setPrescriptionId,
        int setNumber,
        int? targetRepsMin = null,
        int? targetRepsMax = null,
        decimal? loadValue = null,
        PrescriptionLoadUnit loadUnit = PrescriptionLoadUnit.Kilograms,
        TimeSpan? restSeconds = null,
        SetTechnique? technique = null,
        int? tempoTutSeconds = null,
        string? notes = null)
    {
        var set = _prescriptions.FirstOrDefault(p => p.Id == setPrescriptionId);
        if (set == null)
            throw new InvalidOperationException($"Set prescription {setPrescriptionId} not found.");

        if (setNumber != set.SetNumber && _prescriptions.Any(p => p.SetNumber == setNumber && p.Id != setPrescriptionId))
            throw new InvalidOperationException($"Set number {setNumber} already exists in this exercise.");

        var techniqueToUse = technique ?? GlobalSetTechniqueAppliedToAllSets ?? SetTechnique.Standard;
        TimeSpan? tut = tempoTutSeconds.HasValue ? TimeSpan.FromSeconds(tempoTutSeconds.Value) : null;

        set.Update(
            setNumber,
            targetRepsMin,
            targetRepsMax,
            loadValue,
            loadUnit,
            restSeconds,
            techniqueToUse,
            rateOfPerceivedExertionRPE: null,
            repsInReserveRIR: null,
            tempoUnderTensionTUTSeconds: tut,
            notesProfessor: notes,
            tempoNotation: null);

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemovePrescriptionSet(Guid setPrescriptionId)
    {
        var set = _prescriptions.FirstOrDefault(p => p.Id == setPrescriptionId);
        if (set == null) return;
        _prescriptions.Remove(set);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReorderSets(Dictionary<Guid, int> setOrders)
    {
        if (setOrders == null)
            throw new ArgumentNullException(nameof(setOrders));

        var usedNumbers = new HashSet<int>();
        foreach (var kvp in setOrders)
        {
            if (!usedNumbers.Add(kvp.Value))
                throw new InvalidOperationException($"Duplicate set number {kvp.Value} in reorder.");

            var set = _prescriptions.FirstOrDefault(p => p.Id == kvp.Key);
            if (set != null)
                set.SetNumberUpdate(kvp.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void ImportPrescriptions(List<SetPrescription> copiedSets)
    {
        _prescriptions.AddRange(copiedSets);
    }
}
