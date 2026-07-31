namespace TreviaApp.Contracts.Profiles.Requests;

public sealed record UpsertMeasurementRequest(
    long? Id,
    DateTimeOffset MeasuredAt,
    decimal? HeightCm = null,
    decimal? WaistCm = null,
    decimal? HipCm = null,
    decimal? ChestCm = null,
    decimal? ArmLeftCm = null,
    decimal? ArmRightCm = null,
    decimal? ThighLeftCm = null,
    decimal? ThighRightCm = null,
    decimal? CalfLeftCm = null,
    decimal? CalfRightCm = null,
    decimal? BodyFatPercent = null,
    decimal? WaterPercent = null,
    decimal? MuscleMassPercent = null,
    decimal? VisceralFatRating = null,
    decimal? BmiKgM2 = null,
    string? Note = null);
