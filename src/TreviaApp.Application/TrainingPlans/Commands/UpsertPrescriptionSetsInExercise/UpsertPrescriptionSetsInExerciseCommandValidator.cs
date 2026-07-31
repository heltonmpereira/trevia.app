namespace TreviaApp.Application.TrainingPlans.Commands.UpsertPrescriptionSetsInExercise;

using FluentValidation;

public sealed class UpsertPrescriptionSetsInExerciseCommandValidator : AbstractValidator<UpsertPrescriptionSetsInExerciseCommand>
{
    public UpsertPrescriptionSetsInExerciseCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.SessionExerciseId)
            .NotEmpty();

        RuleFor(x => x.Sets)
            .NotEmpty();

        RuleForEach(x => x.Sets)
            .ChildRules(set =>
            {
                set.RuleFor(s => s.SetNumber)
                    .GreaterThanOrEqualTo(1);

                set.RuleFor(s => s.LoadValue)
                    .GreaterThanOrEqualTo(0)
                    .When(s => s.LoadValue.HasValue);

                set.RuleFor(s => s.RPE)
                    .InclusiveBetween(1, 10)
                    .When(s => s.RPE.HasValue);

                set.RuleFor(s => s.RepsInReserveRIR)
                    .InclusiveBetween(0, 5)
                    .When(s => s.RepsInReserveRIR.HasValue);

                set.RuleFor(s => s.TempoNotation)
                    .MaximumLength(10);

                set.RuleFor(s => s.NotesProfessor)
                    .MaximumLength(500);
            });
    }
}
