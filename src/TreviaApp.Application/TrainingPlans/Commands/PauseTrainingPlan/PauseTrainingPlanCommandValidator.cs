namespace TreviaApp.Application.TrainingPlans.Commands.PauseTrainingPlan;

using FluentValidation;

public sealed class PauseTrainingPlanCommandValidator : AbstractValidator<PauseTrainingPlanCommand>
{
    public PauseTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
