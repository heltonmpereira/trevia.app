namespace TreviaApp.Application.Coaching.Commands.CancelCoachInvite;

using FluentValidation;

public sealed class CancelCoachInviteCommandValidator : AbstractValidator<CancelCoachInviteCommand>
{
    public CancelCoachInviteCommandValidator()
    {
        RuleFor(x => x.InviteId)
            .NotEmpty();
    }
}
