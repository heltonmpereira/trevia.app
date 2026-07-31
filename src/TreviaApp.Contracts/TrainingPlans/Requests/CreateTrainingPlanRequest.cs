using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record CreateTrainingPlanRequest(
    string Name,
    string? Description,
    string? InstructionsIntro,
    string? NotesForStudent,
    TrainingSplitType SplitType,
    Visibility Visibility = Visibility.Private,
    int? TotalWeeks = null,
    int? SessionsPerWeek = null,
    decimal? TargetVolume = null,
    string? Tags = null);
