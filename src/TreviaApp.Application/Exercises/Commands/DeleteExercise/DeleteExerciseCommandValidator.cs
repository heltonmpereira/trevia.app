namespace TreviaApp.Application.Exercises.Commands.DeleteExercise;

using FluentValidation;

public sealed class DeleteExerciseCommandValidator : AbstractValidator<DeleteExerciseCommand>
{
    public DeleteExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
    }
}
