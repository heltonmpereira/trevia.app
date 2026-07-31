namespace TreviaApp.Application.Exercises.Commands.AddMuscleToExercise;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class AddMuscleToExerciseCommandValidator : AbstractValidator<AddMuscleToExerciseCommand>
{
    public AddMuscleToExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Muscle).IsInEnum();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.ActivationPercent)
            .InclusiveBetween(0, 100)
            .When(x => x.ActivationPercent.HasValue);
    }
}
