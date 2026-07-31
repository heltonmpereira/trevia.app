namespace TreviaApp.Application.Coaching.Commands.SendCoachInvite;

using FluentValidation;

public sealed class SendCoachInviteCommandValidator : AbstractValidator<SendCoachInviteCommand>
{
    public SendCoachInviteCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.Message)
            .MaximumLength(500);

        RuleFor(x => x.ExpiresInDays)
            .GreaterThan(0);
    }
}
