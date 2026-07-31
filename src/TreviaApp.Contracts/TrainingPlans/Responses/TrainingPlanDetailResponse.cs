using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

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
