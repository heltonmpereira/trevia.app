namespace TreviaApp.Application.TrainingPlans.Commands.PublishTrainingPlan;

using FluentValidation;

public sealed class PublishTrainingPlanCommandValidator : AbstractValidator<PublishTrainingPlanCommand>
{
    public PublishTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
