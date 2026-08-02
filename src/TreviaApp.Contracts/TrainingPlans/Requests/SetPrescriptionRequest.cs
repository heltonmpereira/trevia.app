using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for SetPrescriptionRequest.
/// </summary>
/// <param name="SetNumber">Set Number value.</param>
/// <param name="TargetRepsMin">Target Reps Min value.</param>
/// <param name="TargetRepsMax">Target Reps Max value.</param>
/// <param name="LoadValue">Load Value value.</param>
/// <param name="LoadUnit">Load Unit value.</param>
/// <param name="RestAfterSeconds">Rest After Seconds value.</param>
/// <param name="TechniqueApplied">Technique Applied value.</param>
/// <param name="RPE">RPE value.</param>
/// <param name="RepsInReserveRIR">Reps In Reserve RIR value.</param>
/// <param name="TUTSeconds">TUTSeconds value.</param>
/// <param name="NotesProfessor">Notes Professor value.</param>
/// <param name="TempoNotation">Tempo Notation value.</param>
public sealed record SetPrescriptionRequest(
    int SetNumber,
    int? TargetRepsMin = null,
    int? TargetRepsMax = null,
    decimal? LoadValue = null,
    PrescriptionLoadUnit LoadUnit = PrescriptionLoadUnit.Kilograms,
    TimeSpan? RestAfterSeconds = null,
    SetTechnique TechniqueApplied = SetTechnique.Standard,
    int? RPE = null,
    int? RepsInReserveRIR = null,
    TimeSpan? TUTSeconds = null,
    string? NotesProfessor = null,
    string? TempoNotation = null);
