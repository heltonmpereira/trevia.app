using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.TrainingPlans;

public class SetPrescription : Entity
{
    public Guid SessionExerciseId { get; private set; }
    public SessionExercise SessionExercise { get; private set; } = null!;

    public int SetNumber { get; private set; }
    public int? TargetRepsMin { get; private set; }
    public int? TargetRepsMax { get; private set; }
    public decimal? LoadValue { get; private set; }
    public PrescriptionLoadUnit LoadUnit { get; private set; } = PrescriptionLoadUnit.Kilograms;
    public TimeSpan? RestAfterSeconds { get; private set; }
    public SetTechnique TechniqueApplied { get; private set; } = SetTechnique.Standard;
    public int? RateOfPerceivedExertionRPE { get; private set; }
    public int? RepsInReserveRIR { get; private set; }
    public TimeSpan? TempoUnderTensionTUTSeconds { get; private set; }
    public string? NotesProfessor { get; private set; }
    public string? TempoNotation { get; private set; }

    private SetPrescription() { }

    public SetPrescription(
        Guid sessionExerciseId,
        int setNumber,
        int? targetRepsMin = null,
        int? targetRepsMax = null,
        decimal? loadValue = null,
        PrescriptionLoadUnit loadUnit = PrescriptionLoadUnit.Kilograms,
        TimeSpan? restAfterSeconds = null,
        SetTechnique techniqueApplied = SetTechnique.Standard,
        int? rateOfPerceivedExertionRPE = null,
        int? repsInReserveRIR = null,
        TimeSpan? tempoUnderTensionTUTSeconds = null,
        string? notesProfessor = null,
        string? tempoNotation = null)
    {
        if (sessionExerciseId == Guid.Empty)
            throw new ArgumentException("SessionExerciseId cannot be empty.", nameof(sessionExerciseId));
        if (setNumber < 1)
            throw new ArgumentException("SetNumber must be at least 1.", nameof(setNumber));
        if (rateOfPerceivedExertionRPE is < 1 or > 10)
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rateOfPerceivedExertionRPE));
        if (repsInReserveRIR is < 0 or > 5)
            throw new ArgumentException("RIR must be between 0 and 5.", nameof(repsInReserveRIR));
        if (notesProfessor != null && notesProfessor.Length > 500)
            throw new ArgumentException("NotesProfessor too long (> 500).", nameof(notesProfessor));
        if (tempoNotation != null && tempoNotation.Length > 10)
            throw new ArgumentException("TempoNotation too long (> 10).", nameof(tempoNotation));

        SessionExerciseId = sessionExerciseId;
        SetNumber = setNumber;
        TargetRepsMin = targetRepsMin;
        TargetRepsMax = targetRepsMax;
        LoadValue = loadValue;
        LoadUnit = loadUnit;
        RestAfterSeconds = restAfterSeconds;
        TechniqueApplied = techniqueApplied;
        RateOfPerceivedExertionRPE = rateOfPerceivedExertionRPE;
        RepsInReserveRIR = repsInReserveRIR;
        TempoUnderTensionTUTSeconds = tempoUnderTensionTUTSeconds;
        NotesProfessor = notesProfessor;
        TempoNotation = tempoNotation;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        int setNumber,
        int? targetRepsMin,
        int? targetRepsMax,
        decimal? loadValue,
        PrescriptionLoadUnit loadUnit,
        TimeSpan? restAfterSeconds,
        SetTechnique techniqueApplied,
        int? rateOfPerceivedExertionRPE,
        int? repsInReserveRIR,
        TimeSpan? tempoUnderTensionTUTSeconds,
        string? notesProfessor,
        string? tempoNotation)
    {
        if (setNumber < 1)
            throw new ArgumentException("SetNumber must be at least 1.", nameof(setNumber));
        if (rateOfPerceivedExertionRPE is < 1 or > 10)
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rateOfPerceivedExertionRPE));
        if (repsInReserveRIR is < 0 or > 5)
            throw new ArgumentException("RIR must be between 0 and 5.", nameof(repsInReserveRIR));
        if (notesProfessor != null && notesProfessor.Length > 500)
            throw new ArgumentException("NotesProfessor too long (> 500).", nameof(notesProfessor));
        if (tempoNotation != null && tempoNotation.Length > 10)
            throw new ArgumentException("TempoNotation too long (> 10).", nameof(tempoNotation));

        SetNumber = setNumber;
        TargetRepsMin = targetRepsMin;
        TargetRepsMax = targetRepsMax;
        LoadValue = loadValue;
        LoadUnit = loadUnit;
        RestAfterSeconds = restAfterSeconds;
        TechniqueApplied = techniqueApplied;
        RateOfPerceivedExertionRPE = rateOfPerceivedExertionRPE;
        RepsInReserveRIR = repsInReserveRIR;
        TempoUnderTensionTUTSeconds = tempoUnderTensionTUTSeconds;
        NotesProfessor = notesProfessor;
        TempoNotation = tempoNotation;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetNumberUpdate(int newSetNumber)
    {
        if (newSetNumber < 1)
            throw new ArgumentException("SetNumber must be at least 1.", nameof(newSetNumber));
        SetNumber = newSetNumber;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
