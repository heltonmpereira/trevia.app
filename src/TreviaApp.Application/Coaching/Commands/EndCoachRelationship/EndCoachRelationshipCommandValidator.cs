namespace TreviaApp.Application.Coaching.Commands.EndCoachRelationship;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class EndCoachRelationshipCommandValidator : AbstractValidator<EndCoachRelationshipCommand>
{
    public EndCoachRelationshipCommandValidator()
    {
        RuleFor(x => x.LinkId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .IsInEnum();

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}
