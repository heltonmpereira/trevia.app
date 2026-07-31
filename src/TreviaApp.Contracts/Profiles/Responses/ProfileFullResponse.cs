using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record ProfileFullResponse(
    Guid Id,
    Guid UserId,
    string? DisplayName,
    string? Bio,
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    PrivacyLevel PrivacyLevel,
    string PreferredUnits,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ProfilePhotoResponse? Photo,
    List<Equipment> Equipments,
    int TotalWeighIns,
    int TotalMeasurements,
    decimal? LatestWeightKg,
    DateTimeOffset? LatestWeightAt,
    decimal? LatestHeightCm,
    decimal? LatestBodyFatPercent);
