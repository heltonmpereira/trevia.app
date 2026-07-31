namespace TreviaApp.Application.TrainingPlans.Commands.UpdateExerciseInSession;

using FluentValidation;

public sealed class UpdateExerciseInSessionCommandValidator : AbstractValidator<UpdateExerciseInSessionCommand>
{
    public UpdateExerciseInSessionCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.SessionExerciseId)
            .NotEmpty();

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1);
    }
}
