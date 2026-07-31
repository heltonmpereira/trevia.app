namespace TreviaApp.Application.Authentication.Commands.ChangePassword;
using FluentValidation;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128).NotEqual(x => x.CurrentPassword).WithMessage("Nova senha deve ser diferente da atual.");
        RuleFor(x => x.ConfirmNewPassword).NotEmpty().Equal(x => x.NewPassword);
    }
}
