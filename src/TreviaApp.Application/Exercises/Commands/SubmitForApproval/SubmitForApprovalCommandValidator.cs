namespace TreviaApp.Application.Exercises.Commands.SubmitForApproval;

using FluentValidation;

public sealed class SubmitForApprovalCommandValidator : AbstractValidator<SubmitForApprovalCommand>
{
    public SubmitForApprovalCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
    }
}
