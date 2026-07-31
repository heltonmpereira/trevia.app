using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

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
