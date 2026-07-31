using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.Profiles;

public class PhysicalMeasurement : Entity
{
    public Guid ProfileId { get; private set; }
    public UserProfile Profile { get; private set; } = null!;
    public DateTimeOffset MeasuredAt { get; private set; }

    public decimal? HeightCm { get; private set; }

    public decimal? WaistCm { get; private set; }
    public decimal? HipCm { get; private set; }
    public decimal? ChestCm { get; private set; }
    public decimal? ArmLeftCm { get; private set; }
    public decimal? ArmRightCm { get; private set; }
    public decimal? ThighLeftCm { get; private set; }
    public decimal? ThighRightCm { get; private set; }
    public decimal? CalfLeftCm { get; private set; }
    public decimal? CalfRightCm { get; private set; }

    public decimal? BodyFatPercent { get; private set; }
    public decimal? WaterPercent { get; private set; }
    public decimal? MuscleMassPercent { get; private set; }
    public decimal? VisceralFatRating { get; private set; }
    public decimal? BmiKgM2 { get; private set; }

    public string? Note { get; private set; }

    private PhysicalMeasurement() { }

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
