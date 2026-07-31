namespace TreviaApp.Application.Exercises.Commands.RemoveEquipmentFromExercise;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class RemoveEquipmentFromExerciseCommandValidator : AbstractValidator<RemoveEquipmentFromExerciseCommand>
{
    public RemoveEquipmentFromExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Equipment).IsInEnum();
    }
}
