namespace TreviaApp.Application.TrainingPlans.Commands.RemoveExerciseFromSession;

using FluentValidation;

public sealed class RemoveExerciseFromSessionCommandValidator : AbstractValidator<RemoveExerciseFromSessionCommand>
{
    public RemoveExerciseFromSessionCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.SessionExerciseId)
            .NotEmpty();
    }
}
