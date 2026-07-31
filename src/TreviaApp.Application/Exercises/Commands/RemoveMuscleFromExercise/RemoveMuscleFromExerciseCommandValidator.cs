namespace TreviaApp.Application.Exercises.Commands.RemoveMuscleFromExercise;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class RemoveMuscleFromExerciseCommandValidator : AbstractValidator<RemoveMuscleFromExerciseCommand>
{
    public RemoveMuscleFromExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Muscle).IsInEnum();
    }
}
