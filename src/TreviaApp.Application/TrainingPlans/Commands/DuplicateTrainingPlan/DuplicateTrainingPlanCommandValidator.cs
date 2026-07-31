namespace TreviaApp.Application.TrainingPlans.Commands.DuplicateTrainingPlan;

using FluentValidation;

public sealed class DuplicateTrainingPlanCommandValidator : AbstractValidator<DuplicateTrainingPlanCommand>
{
    public DuplicateTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
