namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record MeasurementResponse(
    long Id,
    DateTimeOffset MeasuredAt,
    decimal? HeightCm,
    decimal? WaistCm,
    decimal? HipCm,
    decimal? ChestCm,
    decimal? ArmLeftCm,
    decimal? ArmRightCm,
    decimal? ThighLeftCm,
    decimal? ThighRightCm,
    decimal? CalfLeftCm,
    decimal? CalfRightCm,
    decimal? BodyFatPercent,
    decimal? WaterPercent,
    decimal? MuscleMassPercent,
    decimal? VisceralFatRating,
    decimal? BmiKgM2,
    string? Note,
    DateTimeOffset CreatedAt);
