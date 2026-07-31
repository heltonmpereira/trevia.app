namespace TreviaApp.Application.Coaching.Commands.UpdateCoachPermissions;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class UpdateCoachPermissionsCommandValidator : AbstractValidator<UpdateCoachPermissionsCommand>
{
    public UpdateCoachPermissionsCommandValidator()
    {
        RuleFor(x => x.LinkId)
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .IsInEnum();
    }
}
