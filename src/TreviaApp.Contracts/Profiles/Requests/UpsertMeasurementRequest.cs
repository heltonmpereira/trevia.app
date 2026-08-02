namespace TreviaApp.Contracts.Profiles.Requests;

/// <summary>
/// Request payload for UpsertMeasurementRequest.
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
