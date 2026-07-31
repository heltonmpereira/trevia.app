namespace TreviaApp.Application.Coaching.Commands.RejectCoachInvite;

using FluentValidation;

public sealed class RejectCoachInviteCommandValidator : AbstractValidator<RejectCoachInviteCommand>
{
    public RejectCoachInviteCommandValidator()
    {
        RuleFor(x => x.InviteId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}
