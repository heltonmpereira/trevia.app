namespace TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingPlan;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class UpdateTrainingPlanCommandValidator : AbstractValidator<UpdateTrainingPlanCommand>
{
    public UpdateTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.InstructionsIntro)
            .MaximumLength(2000);

        RuleFor(x => x.NotesForStudent)
            .MaximumLength(2000);

        RuleFor(x => x.Tags)
            .MaximumLength(500);

        RuleFor(x => x.SplitType)
            .IsInEnum();

        RuleFor(x => x.Visibility)
            .IsInEnum();

        RuleFor(x => x.TotalWeeks)
            .GreaterThanOrEqualTo(1)
            .When(x => x.TotalWeeks.HasValue);

        RuleFor(x => x.SessionsPerWeek)
            .GreaterThanOrEqualTo(1)
            .When(x => x.SessionsPerWeek.HasValue);
    }
}
