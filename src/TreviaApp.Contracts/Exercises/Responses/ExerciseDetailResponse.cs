using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

/// <summary>
/// Response payload for ExerciseDetailResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Name">Name value.</param>
/// <param name="Slug">Slug value.</param>
/// <param name="ShortDescription">Short Description value.</param>
/// <param name="Instructions">Instructions value.</param>
/// <param name="Tips">Tips value.</param>
/// <param name="Tags">Tags value.</param>
/// <param name="Environment">Environment value.</param>
/// <param name="Modality">Modality value.</param>
/// <param name="DifficultyLevel">Difficulty Level value.</param>
/// <param name="MeasurementType">Measurement Type value.</param>
/// <param name="Visibility">Visibility value.</param>
/// <param name="Status">Status value.</param>
/// <param name="CreatedByUserId">Created By User Id value.</param>
/// <param name="CreatedByName">Created By Name value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="UpdatedAt">Updated At value.</param>
/// <param name="ApprovedByUserId">Approved By User Id value.</param>
/// <param name="ApprovedByName">Approved By Name value.</param>
/// <param name="ApprovedAt">Approved At value.</param>
/// <param name="RejectReason">Reject Reason value.</param>
/// <param name="RejectedAt">Rejected At value.</param>
/// <param name="Muscles">Muscles value.</param>
/// <param name="Equipments">Equipments value.</param>
/// <param name="Medias">Medias value.</param>
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
