using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

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
