using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record UpdateTrainingPlanRequest(
    string Name,
    string? Description,
    string? InstructionsIntro,
    string? NotesForStudent,
    TrainingSplitType SplitType,
    Visibility Visibility,
    int? TotalWeeks,
    int? SessionsPerWeek,
    decimal? TargetVolume,
    string? Tags);
