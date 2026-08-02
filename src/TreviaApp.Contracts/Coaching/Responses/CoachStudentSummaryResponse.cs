using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

/// <summary>
/// Response payload for CoachStudentSummaryResponse.
/// </summary>
/// <param name="UserId">User Id value.</param>
/// <param name="DisplayName">Display Name value.</param>
/// <param name="PhotoFileId">Photo File Id value.</param>
/// <param name="Goal">Goal value.</param>
/// <param name="Experience">Experience value.</param>
/// <param name="LinkedSince">Linked Since value.</param>
/// <param name="Permissions">Permissions value.</param>
/// <param name="ActiveTrainingPlansCount">Active Training Plans Count value.</param>
public sealed record CoachStudentSummaryResponse(
    Guid UserId,
    string DisplayName,
    string? PhotoFileId,
    TrainingGoal? Goal,
    ExperienceLevel? Experience,
    DateTimeOffset LinkedSince,
    CoachPermissions Permissions,
    int ActiveTrainingPlansCount);
