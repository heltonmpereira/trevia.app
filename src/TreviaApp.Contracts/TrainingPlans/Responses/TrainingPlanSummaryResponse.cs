using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

/// <summary>
/// Response payload for TrainingPlanSummaryResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Name">Name value.</param>
/// <param name="Description">Description value.</param>
/// <param name="SplitType">Split Type value.</param>
/// <param name="Status">Status value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="IsPublicTemplate">Is Public Template value.</param>
/// <param name="Version">Version value.</param>
/// <param name="TotalSessions">Total Sessions value.</param>
/// <param name="TotalExercises">Total Exercises value.</param>
/// <param name="TotalWeeks">Total Weeks value.</param>
/// <param name="SessionsPerWeek">Sessions Per Week value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="UpdatedAt">Updated At value.</param>
/// <param name="CreatedByName">Created By Name value.</param>
/// <param name="AssignedToStudentId">Assigned To Student Id value.</param>
/// <param name="AssignedToStudentName">Assigned To Student Name value.</param>
/// <param name="AssignedAt">Assigned At value.</param>
public sealed record TrainingPlanSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    TrainingSplitType SplitType,
    TrainingPlanStatus Status,
    Visibility Visibility,
    bool IsPublicTemplate,
    int Version,
    int TotalSessions,
    int TotalExercises,
    int? TotalWeeks,
    int? SessionsPerWeek,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CreatedByName,
    Guid? AssignedToStudentId,
    string? AssignedToStudentName,
    DateTimeOffset? AssignedAt);
