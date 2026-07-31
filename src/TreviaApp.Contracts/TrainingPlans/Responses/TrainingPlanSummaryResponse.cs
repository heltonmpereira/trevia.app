using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

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
