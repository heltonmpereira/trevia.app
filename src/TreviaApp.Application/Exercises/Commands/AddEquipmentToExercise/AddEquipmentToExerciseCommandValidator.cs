namespace TreviaApp.Application.Exercises.Commands.AddEquipmentToExercise;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class AddEquipmentToExerciseCommandValidator : AbstractValidator<AddEquipmentToExerciseCommand>
{
    public AddEquipmentToExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Equipment).IsInEnum();
    }
}
