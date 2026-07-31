namespace TreviaApp.Application.TrainingPlans.Commands.CompleteTrainingPlan;

using FluentValidation;

public sealed class CompleteTrainingPlanCommandValidator : AbstractValidator<CompleteTrainingPlanCommand>
{
    public CompleteTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
