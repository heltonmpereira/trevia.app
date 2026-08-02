using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

/// <summary>
/// Response payload for TrainingPlanDetailResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Name">Name value.</param>
/// <param name="Description">Description value.</param>
/// <param name="InstructionsIntro">Instructions Intro value.</param>
/// <param name="NotesForStudent">Notes For Student value.</param>
/// <param name="SplitType">Split Type value.</param>
/// <param name="Status">Status value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="IsPublicTemplate">Is Public Template value.</param>
/// <param name="Version">Version value.</param>
/// <param name="TotalWeeks">Total Weeks value.</param>
/// <param name="SessionsPerWeek">Sessions Per Week value.</param>
/// <param name="TargetVolume">Target Volume value.</param>
/// <param name="Tags">Tags value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="UpdatedAt">Updated At value.</param>
/// <param name="CreatedByUserId">Created By User Id value.</param>
/// <param name="CreatedByName">Created By Name value.</param>
/// <param name="PublishedAt">Published At value.</param>
/// <param name="AssignedAt">Assigned At value.</param>
/// <param name="CompletedAt">Completed At value.</param>
/// <param name="AssignedToStudentId">Assigned To Student Id value.</param>
/// <param name="AssignedToStudentName">Assigned To Student Name value.</param>
/// <param name="Sessions">Sessions value.</param>
/// <param name="ActualCompletedVolumeKgEstimate">Actual Completed Volume Kg Estimate value.</param>
public sealed record TrainingPlanDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string? InstructionsIntro,
    string? NotesForStudent,
    TrainingSplitType SplitType,
    TrainingPlanStatus Status,
    Visibility Visibility,
    bool IsPublicTemplate,
    int Version,
    int? TotalWeeks,
    int? SessionsPerWeek,
    decimal? TargetVolume,
    string? Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid CreatedByUserId,
    string? CreatedByName,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? CompletedAt,
    Guid? AssignedToStudentId,
    string? AssignedToStudentName,
    IReadOnlyList<TrainingSessionResponse> Sessions,
    decimal? ActualCompletedVolumeKgEstimate = null);
