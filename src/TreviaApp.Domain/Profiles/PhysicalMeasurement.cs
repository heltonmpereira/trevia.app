using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.Profiles;

/// <summary>
/// Represents the PhysicalMeasurement domain entity.
/// </summary>
public class PhysicalMeasurement : Entity
{
    /// <summary>
    /// Gets Profile Id.
    /// </summary>
    public Guid ProfileId { get; private set; }
    /// <summary>
    /// Gets Profile.
    /// </summary>
    public UserProfile Profile { get; private set; } = null!;
    /// <summary>
    /// Gets Measured At.
    /// </summary>
    public DateTimeOffset MeasuredAt { get; private set; }

    /// <summary>
    /// Gets Height Cm.
    /// </summary>
    public decimal? HeightCm { get; private set; }

    /// <summary>
    /// Gets Waist Cm.
    /// </summary>
    public decimal? WaistCm { get; private set; }
    /// <summary>
    /// Gets Hip Cm.
    /// </summary>
    public decimal? HipCm { get; private set; }
    /// <summary>
    /// Gets Chest Cm.
    /// </summary>
    public decimal? ChestCm { get; private set; }
    /// <summary>
    /// Gets Arm Left Cm.
    /// </summary>
    public decimal? ArmLeftCm { get; private set; }
    /// <summary>
    /// Gets Arm Right Cm.
    /// </summary>
    public decimal? ArmRightCm { get; private set; }
    /// <summary>
    /// Gets Thigh Left Cm.
    /// </summary>
    public decimal? ThighLeftCm { get; private set; }
    /// <summary>
    /// Gets Thigh Right Cm.
    /// </summary>
    public decimal? ThighRightCm { get; private set; }
    /// <summary>
    /// Gets Calf Left Cm.
    /// </summary>
    public decimal? CalfLeftCm { get; private set; }
    /// <summary>
    /// Gets Calf Right Cm.
    /// </summary>
    public decimal? CalfRightCm { get; private set; }

    /// <summary>
    /// Gets Body Fat Percent.
    /// </summary>
    public decimal? BodyFatPercent { get; private set; }
    /// <summary>
    /// Gets Water Percent.
    /// </summary>
    public decimal? WaterPercent { get; private set; }
    /// <summary>
    /// Gets Muscle Mass Percent.
    /// </summary>
    public decimal? MuscleMassPercent { get; private set; }
    /// <summary>
    /// Gets Visceral Fat Rating.
    /// </summary>
    public decimal? VisceralFatRating { get; private set; }
    /// <summary>
    /// Gets Bmi Kg M2.
    /// </summary>
    public decimal? BmiKgM2 { get; private set; }

    /// <summary>
    /// Gets Note.
    /// </summary>
    public string? Note { get; private set; }

    private PhysicalMeasurement() { }

    /// <summary>
    /// Initializes a new instance of the PhysicalMeasurement class.
    /// </summary>
    public PhysicalMeasurement(
        Guid profileId,
        DateTimeOffset measuredAt,
        decimal? heightCm = null,
        decimal? waistCm = null,
        decimal? hipCm = null,
        decimal? chestCm = null,
        decimal? armLeftCm = null,
        decimal? armRightCm = null,
        decimal? thighLeftCm = null,
        decimal? thighRightCm = null,
        decimal? calfLeftCm = null,
        decimal? calfRightCm = null,
        decimal? bodyFatPercent = null,
        decimal? waterPercent = null,
        decimal? muscleMassPercent = null,
        decimal? visceralFatRating = null,
        decimal? bmiKgM2 = null,
        string? note = null)
    {
        ProfileId = profileId;
        MeasuredAt = measuredAt;
        HeightCm = Round(heightCm);
        WaistCm = Round(waistCm);
        HipCm = Round(hipCm);
        ChestCm = Round(chestCm);
        ArmLeftCm = Round(armLeftCm);
        ArmRightCm = Round(armRightCm);
        ThighLeftCm = Round(thighLeftCm);
        ThighRightCm = Round(thighRightCm);
        CalfLeftCm = Round(calfLeftCm);
        CalfRightCm = Round(calfRightCm);
        BodyFatPercent = Round(bodyFatPercent, 1);
        WaterPercent = Round(waterPercent, 1);
        MuscleMassPercent = Round(muscleMassPercent, 1);
        VisceralFatRating = Round(visceralFatRating, 1);
        BmiKgM2 = Round(bmiKgM2, 2);
        Note = note;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static decimal? Round(decimal? value, int decimals = 2)
        => value.HasValue ? Math.Round(value.Value, decimals) : null;
}
