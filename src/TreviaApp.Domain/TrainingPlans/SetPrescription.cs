using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.TrainingPlans;

/// <summary>
/// Represents the SetPrescription domain entity.
/// </summary>
public class SetPrescription : Entity
{
    /// <summary>
    /// Gets Session Exercise Id.
    /// </summary>
    public Guid SessionExerciseId { get; private set; }
    /// <summary>
    /// Gets Session Exercise.
    /// </summary>
    public SessionExercise SessionExercise { get; private set; } = null!;

    /// <summary>
    /// Gets Set Number.
    /// </summary>
    public int SetNumber { get; private set; }
    /// <summary>
    /// Gets Target Reps Min.
    /// </summary>
    public int? TargetRepsMin { get; private set; }
    /// <summary>
    /// Gets Target Reps Max.
    /// </summary>
    public int? TargetRepsMax { get; private set; }
    /// <summary>
    /// Gets Load Value.
    /// </summary>
    public decimal? LoadValue { get; private set; }
    /// <summary>
    /// Gets Load Unit.
    /// </summary>
    public PrescriptionLoadUnit LoadUnit { get; private set; } = PrescriptionLoadUnit.Kilograms;
    /// <summary>
    /// Gets Rest After Seconds.
    /// </summary>
    public TimeSpan? RestAfterSeconds { get; private set; }
    /// <summary>
    /// Gets Technique Applied.
    /// </summary>
    public SetTechnique TechniqueApplied { get; private set; } = SetTechnique.Standard;
    /// <summary>
    /// Gets Rate Of Perceived Exertion RPE.
    /// </summary>
    public int? RateOfPerceivedExertionRPE { get; private set; }
    /// <summary>
    /// Gets Reps In Reserve RIR.
    /// </summary>
    public int? RepsInReserveRIR { get; private set; }
    /// <summary>
    /// Gets Tempo Under Tension TUTSeconds.
    /// </summary>
    public TimeSpan? TempoUnderTensionTUTSeconds { get; private set; }
    /// <summary>
    /// Gets Notes Professor.
    /// </summary>
    public string? NotesProfessor { get; private set; }
    /// <summary>
    /// Gets Tempo Notation.
    /// </summary>
    public string? TempoNotation { get; private set; }

    private SetPrescription() { }

    /// <summary>
    /// Initializes a new instance of the SetPrescription class.
    /// </summary>
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

    /// <summary>
    /// Executes Update.
    /// </summary>
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

    /// <summary>
    /// Executes Set Number Update.
    /// </summary>
    public void SetNumberUpdate(int newSetNumber)
    {
        if (newSetNumber < 1)
            throw new ArgumentException("SetNumber must be at least 1.", nameof(newSetNumber));
        SetNumber = newSetNumber;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
