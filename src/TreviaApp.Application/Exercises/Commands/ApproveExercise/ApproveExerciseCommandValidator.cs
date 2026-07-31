namespace TreviaApp.Application.Exercises.Commands.ApproveExercise;

using FluentValidation;

public sealed class ApproveExerciseCommandValidator : AbstractValidator<ApproveExerciseCommand>
{
    public ApproveExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
    }
}
