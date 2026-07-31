namespace TreviaApp.Application.Coaching.Commands.AcceptCoachInvite;

using FluentValidation;

public sealed class AcceptCoachInviteCommandValidator : AbstractValidator<AcceptCoachInviteCommand>
{
    public AcceptCoachInviteCommandValidator()
    {
        RuleFor(x => x.InviteId)
            .NotEmpty();
    }
}
