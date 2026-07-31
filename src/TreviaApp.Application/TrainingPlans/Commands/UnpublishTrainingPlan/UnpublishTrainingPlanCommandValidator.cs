namespace TreviaApp.Application.TrainingPlans.Commands.UnpublishTrainingPlan;

using FluentValidation;

public sealed class UnpublishTrainingPlanCommandValidator : AbstractValidator<UnpublishTrainingPlanCommand>
{
    public UnpublishTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
