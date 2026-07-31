namespace TreviaApp.Application.TrainingPlans.Commands.RemoveTrainingSession;

using FluentValidation;

public sealed class RemoveTrainingSessionCommandValidator : AbstractValidator<RemoveTrainingSessionCommand>
{
    public RemoveTrainingSessionCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();
    }
}
