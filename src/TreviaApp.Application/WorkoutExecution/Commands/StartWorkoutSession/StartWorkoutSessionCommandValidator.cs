using FluentValidation;

namespace TreviaApp.Application.WorkoutExecution.Commands.StartWorkoutSession;

public sealed class StartWorkoutSessionCommandValidator : AbstractValidator<StartWorkoutSessionCommand>
{
    public StartWorkoutSessionCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.TrainingSessionId).NotEmpty();
        RuleFor(x => x.WeekNumberInPlan).GreaterThanOrEqualTo(1);
    }
}
