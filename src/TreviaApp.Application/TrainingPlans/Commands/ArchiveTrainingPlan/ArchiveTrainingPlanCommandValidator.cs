namespace TreviaApp.Application.TrainingPlans.Commands.ArchiveTrainingPlan;

using FluentValidation;

public sealed class ArchiveTrainingPlanCommandValidator : AbstractValidator<ArchiveTrainingPlanCommand>
{
    public ArchiveTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();
    }
}
