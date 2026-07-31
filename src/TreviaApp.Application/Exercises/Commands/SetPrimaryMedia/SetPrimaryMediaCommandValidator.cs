namespace TreviaApp.Application.Exercises.Commands.SetPrimaryMedia;

using FluentValidation;

public sealed class SetPrimaryMediaCommandValidator : AbstractValidator<SetPrimaryMediaCommand>
{
    public SetPrimaryMediaCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.MediaId).NotEmpty();
    }
}
