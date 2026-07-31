namespace TreviaApp.Application.TrainingPlans.Commands.AddExerciseToTrainingSession;

using FluentValidation;
using TreviaApp.Contracts.TrainingPlans.Requests;

public sealed class AddExerciseToTrainingSessionCommandValidator : AbstractValidator<AddExerciseToTrainingSessionCommand>
{
    public AddExerciseToTrainingSessionCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.ExerciseId)
            .NotEmpty();

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1);

        RuleForEach(x => x.InitialSets)
            .ChildRules(set =>
            {
                set.RuleFor(s => s.SetNumber)
                    .GreaterThanOrEqualTo(1);

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
