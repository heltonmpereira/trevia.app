namespace TreviaApp.Application.TrainingPlans.Commands.ReorderExercisesInSession;

using FluentValidation;

public sealed class ReorderExercisesInSessionCommandValidator : AbstractValidator<ReorderExercisesInSessionCommand>
{
    public ReorderExercisesInSessionCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.Orders)
            .NotEmpty();
    }
}
