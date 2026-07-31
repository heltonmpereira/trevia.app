using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

public sealed record ExerciseDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string Instructions,
    string? Tips,
    string? Tags,
    TrainingEnvironment Environment,
    ExerciseModality Modality,
    DifficultyLevel DifficultyLevel,
    MeasurementType MeasurementType,
    Visibility Visibility,
    ExerciseStatus Status,
    Guid CreatedByUserId,
    string? CreatedByName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? ApprovedByUserId,
    string? ApprovedByName,
    DateTimeOffset? ApprovedAt,
    string? RejectReason,
    DateTimeOffset? RejectedAt,
    List<ExerciseMuscleResponse> Muscles,
    List<ExerciseEquipmentResponse> Equipments,
    List<ExerciseMediaResponse> Medias);
