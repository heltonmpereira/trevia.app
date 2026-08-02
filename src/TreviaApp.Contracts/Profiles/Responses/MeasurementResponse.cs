namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for MeasurementResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="MeasuredAt">Measured At value.</param>
/// <param name="HeightCm">Height Cm value.</param>
/// <param name="WaistCm">Waist Cm value.</param>
/// <param name="HipCm">Hip Cm value.</param>
/// <param name="ChestCm">Chest Cm value.</param>
/// <param name="ArmLeftCm">Arm Left Cm value.</param>
/// <param name="ArmRightCm">Arm Right Cm value.</param>
/// <param name="ThighLeftCm">Thigh Left Cm value.</param>
/// <param name="ThighRightCm">Thigh Right Cm value.</param>
/// <param name="CalfLeftCm">Calf Left Cm value.</param>
/// <param name="CalfRightCm">Calf Right Cm value.</param>
/// <param name="BodyFatPercent">Body Fat Percent value.</param>
/// <param name="WaterPercent">Water Percent value.</param>
/// <param name="MuscleMassPercent">Muscle Mass Percent value.</param>
/// <param name="VisceralFatRating">Visceral Fat Rating value.</param>
/// <param name="BmiKgM2">Bmi Kg M2 value.</param>
/// <param name="Note">Note value.</param>
/// <param name="CreatedAt">Created At value.</param>
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
