namespace TreviaApp.Application.Coaching.Commands.SendStudentRequestToCoach;

using FluentValidation;

public sealed class SendStudentRequestToCoachCommandValidator : AbstractValidator<SendStudentRequestToCoachCommand>
{
    public SendStudentRequestToCoachCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty();

        RuleFor(x => x.Message)
            .MaximumLength(500);

        RuleFor(x => x.ExpiresInDays)
            .GreaterThan(0);
    }
}
