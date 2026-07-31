namespace TreviaApp.Application.Profiles.Commands.UpsertMeasurement;

using FluentValidation;

public sealed class UpsertMeasurementCommandValidator : AbstractValidator<UpsertMeasurementCommand>
{
    public UpsertMeasurementCommandValidator()
    {
        RuleFor(x => x.MeasuredAt)
            .Must(dt => dt <= DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("Data da medida não pode estar no futuro.");

        RuleFor(x => x.HeightCm)
            .GreaterThan(0)
            .When(x => x.HeightCm.HasValue);

        RuleFor(x => x.WaistCm)
            .GreaterThan(0)
            .When(x => x.WaistCm.HasValue);

        RuleFor(x => x.HipCm)
            .GreaterThan(0)
            .When(x => x.HipCm.HasValue);

        RuleFor(x => x.ChestCm)
            .GreaterThan(0)
            .When(x => x.ChestCm.HasValue);

        RuleFor(x => x.ArmLeftCm)
            .GreaterThan(0)
            .When(x => x.ArmLeftCm.HasValue);

        RuleFor(x => x.ArmRightCm)
            .GreaterThan(0)
            .When(x => x.ArmRightCm.HasValue);

        RuleFor(x => x.ThighLeftCm)
            .GreaterThan(0)
            .When(x => x.ThighLeftCm.HasValue);

        RuleFor(x => x.ThighRightCm)
            .GreaterThan(0)
            .When(x => x.ThighRightCm.HasValue);

        RuleFor(x => x.CalfLeftCm)
            .GreaterThan(0)
            .When(x => x.CalfLeftCm.HasValue);

        RuleFor(x => x.CalfRightCm)
            .GreaterThan(0)
            .When(x => x.CalfRightCm.HasValue);

        RuleFor(x => x.BodyFatPercent)
            .GreaterThan(0)
            .When(x => x.BodyFatPercent.HasValue);

        RuleFor(x => x.WaterPercent)
            .GreaterThan(0)
            .When(x => x.WaterPercent.HasValue);

        RuleFor(x => x.MuscleMassPercent)
            .GreaterThan(0)
            .When(x => x.MuscleMassPercent.HasValue);

        RuleFor(x => x.VisceralFatRating)
            .GreaterThan(0)
            .When(x => x.VisceralFatRating.HasValue);

        RuleFor(x => x.BmiKgM2)
            .GreaterThan(0)
            .When(x => x.BmiKgM2.HasValue);

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}
