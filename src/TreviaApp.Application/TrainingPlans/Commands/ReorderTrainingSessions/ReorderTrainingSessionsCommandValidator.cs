namespace TreviaApp.Application.TrainingPlans.Commands.ReorderTrainingSessions;

using FluentValidation;

public sealed class ReorderTrainingSessionsCommandValidator : AbstractValidator<ReorderTrainingSessionsCommand>
{
    public ReorderTrainingSessionsCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.Orders)
            .NotEmpty();
    }
}
