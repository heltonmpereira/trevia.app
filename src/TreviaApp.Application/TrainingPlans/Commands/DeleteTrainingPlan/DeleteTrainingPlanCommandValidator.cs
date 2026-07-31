namespace TreviaApp.Application.TrainingPlans.Commands.DeleteTrainingPlan;

using FluentValidation;

public sealed class DeleteTrainingPlanCommandValidator : AbstractValidator<DeleteTrainingPlanCommand>
{
    public DeleteTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
