using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for ProfileFullResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="UserId">User Id value.</param>
/// <param name="DisplayName">Display Name value.</param>
/// <param name="Bio">Bio value.</param>
/// <param name="Goal">Goal value.</param>
/// <param name="Experience">Experience value.</param>
/// <param name="PreferredEnvironment">Preferred Environment value.</param>
/// <param name="PrivacyLevel">Privacy Level value.</param>
/// <param name="PreferredUnits">Preferred Units value.</param>
/// <param name="CreatedAt">Created At value.</param>
/// <param name="UpdatedAt">Updated At value.</param>
/// <param name="Photo">Photo value.</param>
/// <param name="Equipments">Equipments value.</param>
/// <param name="TotalWeighIns">Total Weigh Ins value.</param>
/// <param name="TotalMeasurements">Total Measurements value.</param>
/// <param name="LatestWeightKg">Latest Weight Kg value.</param>
/// <param name="LatestWeightAt">Latest Weight At value.</param>
/// <param name="LatestHeightCm">Latest Height Cm value.</param>
/// <param name="LatestBodyFatPercent">Latest Body Fat Percent value.</param>
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
