namespace TreviaApp.Application.Profiles.Commands.CreateProfile;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.Goal)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Experience)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.PreferredEnvironment)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.PrivacyLevel)
            .IsInEnum();

        RuleFor(x => x.PreferredUnits)
            .MaximumLength(20);

        RuleFor(x => x.Bio)
            .MaximumLength(500);
    }
}
