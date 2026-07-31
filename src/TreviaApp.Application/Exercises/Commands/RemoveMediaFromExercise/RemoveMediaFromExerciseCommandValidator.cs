namespace TreviaApp.Application.Exercises.Commands.RemoveMediaFromExercise;

using FluentValidation;

public sealed class RemoveMediaFromExerciseCommandValidator : AbstractValidator<RemoveMediaFromExerciseCommand>
{
    public RemoveMediaFromExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.MediaId).NotEmpty();
    }
}
