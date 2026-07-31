using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Coaching.Responses;

public sealed record CoachStudentSummaryResponse(
    Guid UserId,
    string DisplayName,
    string? PhotoFileId,
    TrainingGoal? Goal,
    ExperienceLevel? Experience,
    DateTimeOffset LinkedSince,
    CoachPermissions Permissions,
    int ActiveTrainingPlansCount);
