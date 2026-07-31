namespace TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingSession;

using FluentValidation;

public sealed class UpdateTrainingSessionCommandValidator : AbstractValidator<UpdateTrainingSessionCommand>
{
    public UpdateTrainingSessionCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.CoachNotesInternal)
            .MaximumLength(2000);

        RuleFor(x => x.Focus)
            .MaximumLength(500);
    }
}
