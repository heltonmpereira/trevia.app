namespace TreviaApp.Application.Exercises.Commands.UpdateExercise;

using FluentValidation;

public sealed class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
{
    public UpdateExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Instructions)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Tips)
            .MaximumLength(2000);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500);

        RuleFor(x => x.Tags)
            .MaximumLength(500);

        RuleFor(x => x.Environment).IsInEnum();
        RuleFor(x => x.Modality).IsInEnum();
        RuleFor(x => x.DifficultyLevel).IsInEnum();
        RuleFor(x => x.MeasurementType).IsInEnum();
        RuleFor(x => x.Visibility).IsInEnum();
    }
}
