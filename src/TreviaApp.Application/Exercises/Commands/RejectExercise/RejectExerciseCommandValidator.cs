namespace TreviaApp.Application.Exercises.Commands.RejectExercise;

using FluentValidation;

public sealed class RejectExerciseCommandValidator : AbstractValidator<RejectExerciseCommand>
{
    public RejectExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
