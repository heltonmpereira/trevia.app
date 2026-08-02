using FluentValidation;
using static TreviaApp.Application.WorkoutExecution.Commands.PauseResumeFinish;

namespace TreviaApp.Application.WorkoutExecution.Commands;

public sealed class PauseWorkoutSessionCommandValidator : AbstractValidator<PauseWorkoutSessionCommand>
{
    public PauseWorkoutSessionCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.WorkoutSessionId).NotEmpty();
    }
}

public sealed class ResumeWorkoutSessionCommandValidator : AbstractValidator<ResumeWorkoutSessionCommand>
{
    public ResumeWorkoutSessionCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.WorkoutSessionId).NotEmpty();
    }
}

public sealed class FinishWorkoutSessionCommandValidator : AbstractValidator<FinishWorkoutSessionCommand>
{
    public FinishWorkoutSessionCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.WorkoutSessionId).NotEmpty();
        RuleFor(x => x.GeneralNotes).MaximumLength(2000).When(x => x.GeneralNotes != null);
        RuleFor(x => x.CaloriesBurned).GreaterThanOrEqualTo(0).When(x => x.CaloriesBurned.HasValue);
    }
}
