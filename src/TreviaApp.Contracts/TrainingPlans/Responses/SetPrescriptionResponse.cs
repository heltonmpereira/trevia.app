using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

/// <summary>
/// Response payload for SetPrescriptionResponse.
/// </summary>
/// <param name="Id">Id value.</param>
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
public sealed record SetPrescriptionResponse(
    Guid Id,
    int SetNumber,
    int? TargetRepsMin,
    int? TargetRepsMax,
    decimal? LoadValue,
    PrescriptionLoadUnit LoadUnit,
    TimeSpan? RestAfterSeconds,
    SetTechnique TechniqueApplied,
    int? RPE,
    int? RepsInReserveRIR,
    TimeSpan? TUTSeconds,
    string? NotesProfessor,
    string? TempoNotation);
