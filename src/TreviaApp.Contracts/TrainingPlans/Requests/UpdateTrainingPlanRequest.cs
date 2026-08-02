using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for UpdateTrainingPlanRequest.
/// </summary>
/// <param name="Name">Name value.</param>
/// <param name="Description">Description value.</param>
/// <param name="InstructionsIntro">Instructions Intro value.</param>
/// <param name="NotesForStudent">Notes For Student value.</param>
/// <param name="SplitType">Split Type value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="TotalWeeks">Total Weeks value.</param>
/// <param name="SessionsPerWeek">Sessions Per Week value.</param>
/// <param name="TargetVolume">Target Volume value.</param>
/// <param name="Tags">Tags value.</param>
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
