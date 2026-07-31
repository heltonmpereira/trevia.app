namespace TreviaApp.Application.Profiles.Mappings;

using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Enums;

public static class ProfileMappings
{
    public static ProfileFullResponse MapToFullResponse(
        UserProfile profile,
        string? displayName = null,
        int totalWeighIns = 0,
        int totalMeasurements = 0,
        decimal? latestWeightKg = null,
        DateTimeOffset? latestWeightAt = null,
        decimal? latestHeightCm = null,
        decimal? latestBodyFatPercent = null)
    {
        var photo = profile.Photo is not null
            ? new ProfilePhotoResponse(
                profile.Photo.Id,
                profile.Photo.FileName,
                profile.Photo.ContentType,
                profile.Photo.SizeBytes,
                profile.Photo.UploadedAt,
                null)
            : null;

        var equipments = profile.Equipments
            .Select(e => e.Equipment)
            .Distinct()
            .ToList();

        return new ProfileFullResponse(
            profile.Id,
            profile.UserId,
            displayName,
            profile.Bio,
            profile.Goal,
            profile.Experience,
            profile.PreferredEnvironment,
            profile.PrivacyLevel,
            profile.PreferredUnits,
            profile.CreatedAt,
            profile.UpdatedAt ?? profile.CreatedAt,
            photo,
            equipments,
            totalWeighIns,
            totalMeasurements,
            latestWeightKg,
            latestWeightAt,
            latestHeightCm,
            latestBodyFatPercent);
    }

    public static ProfileFullResponse MapToSummaryResponse(
        UserProfile profile,
        string? displayName = null)
    {
        var photo = profile.Photo is not null
            ? new ProfilePhotoResponse(
                profile.Photo.Id,
                profile.Photo.FileName,
                profile.Photo.ContentType,
                profile.Photo.SizeBytes,
                profile.Photo.UploadedAt,
                null)
            : null;

        var equipments = profile.Equipments
            .Select(e => e.Equipment)
            .Distinct()
            .ToList();

        return new ProfileFullResponse(
            profile.Id,
            profile.UserId,
            displayName,
            profile.Bio,
            profile.Goal,
            profile.Experience,
            profile.PreferredEnvironment,
            profile.PrivacyLevel,
            profile.PreferredUnits,
            profile.CreatedAt,
            profile.UpdatedAt ?? profile.CreatedAt,
            photo,
            equipments,
            0,
            0,
            null,
            null,
            null,
            null);
    }

    public static WeightEntryResponse MapToWeightEntryResponse(WeightEntry entry)
    {
        return new WeightEntryResponse(
            BitConverter.ToInt64(entry.Id.ToByteArray(), 0),
            entry.WeightKg,
            entry.MeasuredAt,
            entry.Note,
            entry.CreatedAt);
    }

    public static MeasurementResponse MapToMeasurementResponse(PhysicalMeasurement m)
    {
        return new MeasurementResponse(
            BitConverter.ToInt64(m.Id.ToByteArray(), 0),
            m.MeasuredAt,
            m.HeightCm,
            m.WaistCm,
            m.HipCm,
            m.ChestCm,
            m.ArmLeftCm,
            m.ArmRightCm,
            m.ThighLeftCm,
            m.ThighRightCm,
            m.CalfLeftCm,
            m.CalfRightCm,
            m.BodyFatPercent,
            m.WaterPercent,
            m.MuscleMassPercent,
            m.VisceralFatRating,
            m.BmiKgM2,
            m.Note,
            m.CreatedAt);
    }
}
