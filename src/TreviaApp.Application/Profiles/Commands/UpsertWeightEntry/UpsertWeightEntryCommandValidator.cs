namespace TreviaApp.Application.Profiles.Commands.UpsertWeightEntry;

using FluentValidation;

public sealed class UpsertWeightEntryCommandValidator : AbstractValidator<UpsertWeightEntryCommand>
{
    public UpsertWeightEntryCommandValidator()
    {
        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .LessThan(700);

        RuleFor(x => x.MeasuredAt)
            .Must(dt => dt <= DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("Data de pesagem não pode estar no futuro.");

        RuleFor(x => x.Note)
            .MaximumLength(200);
    }
}
