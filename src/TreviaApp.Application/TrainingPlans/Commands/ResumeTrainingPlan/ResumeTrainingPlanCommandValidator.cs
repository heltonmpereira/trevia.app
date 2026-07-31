namespace TreviaApp.Application.TrainingPlans.Commands.ResumeTrainingPlan;

using FluentValidation;

public sealed class ResumeTrainingPlanCommandValidator : AbstractValidator<ResumeTrainingPlanCommand>
{
    public ResumeTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
