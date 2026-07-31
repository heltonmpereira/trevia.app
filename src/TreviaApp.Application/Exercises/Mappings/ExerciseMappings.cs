namespace TreviaApp.Application.Exercises.Mappings;

using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Domain.Exercises;

public static class ExerciseMappings
{
    public static ExerciseDetailResponse MapToDetail(
        Exercise exercise,
        string? createdByName = null,
        string? approvedByName = null,
        Dictionary<Guid, string>? mediaUrls = null)
    {
        mediaUrls ??= new Dictionary<Guid, string>();

        var muscles = exercise.Muscles
            .Select(MapToMuscleResponse)
            .ToList();

        var equipments = exercise.Equipments
            .Select(MapToEquipmentResponse)
            .ToList();

        var medias = exercise.Medias
            .Select(m => MapToMediaResponse(m, mediaUrls.GetValueOrDefault(m.Id)))
            .ToList();

        return new ExerciseDetailResponse(
            exercise.Id,
            exercise.Name,
            exercise.Slug,
            exercise.ShortDescription,
            exercise.Instructions,
            exercise.Tips,
            exercise.Tags,
            exercise.Environment,
            exercise.Modality,
            exercise.DifficultyLevel,
            exercise.MeasurementType,
            exercise.Visibility,
            exercise.Status,
            exercise.CreatedByUserId,
            createdByName,
            exercise.CreatedAt,
            exercise.UpdatedAt,
            exercise.ApprovedByUserId,
            approvedByName,
            exercise.ApprovedAt,
            exercise.RejectReason,
            exercise.RejectedAt,
            muscles,
            equipments,
            medias);
    }

    public static ExerciseSummaryResponse MapToSummary(Exercise exercise, string? primaryMediaUrl = null)
    {
        var primaryMusclesCount = exercise.Muscles.Count(m =>
            m.MuscleRole == Shared.Enums.MuscleRole.Primary ||
            m.MuscleRole == Shared.Enums.MuscleRole.Secondary);

        return new ExerciseSummaryResponse(
            exercise.Id,
            exercise.Name,
            exercise.Slug,
            exercise.ShortDescription,
            exercise.Environment,
            exercise.Modality,
            exercise.DifficultyLevel,
            exercise.MeasurementType,
            exercise.Visibility,
            exercise.Status,
            exercise.CreatedAt,
            exercise.UpdatedAt,
            primaryMediaUrl,
            primaryMusclesCount,
            exercise.Equipments.Count);
    }

    public static ExerciseMuscleResponse MapToMuscleResponse(ExerciseMuscle muscle)
    {
        return new ExerciseMuscleResponse(
            muscle.Id,
            muscle.Muscle,
            muscle.MuscleRole,
            muscle.ActivationPercent);
    }

    public static ExerciseEquipmentResponse MapToEquipmentResponse(ExerciseEquipment eq)
    {
        return new ExerciseEquipmentResponse(
            eq.Id,
            eq.Equipment,
            eq.Required);
    }

    public static ExerciseMediaResponse MapToMediaResponse(ExerciseMedia media, string? accessUrl = null)
    {
        return new ExerciseMediaResponse(
            media.Id,
            media.FileName,
            media.MediaType,
            media.Order,
            media.Caption,
            media.IsPrimary,
            media.SizeBytes,
            accessUrl);
    }
}
