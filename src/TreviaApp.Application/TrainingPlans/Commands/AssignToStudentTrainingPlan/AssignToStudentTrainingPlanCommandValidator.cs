namespace TreviaApp.Application.TrainingPlans.Commands.AssignToStudentTrainingPlan;

using FluentValidation;

public sealed class AssignToStudentTrainingPlanCommandValidator : AbstractValidator<AssignToStudentTrainingPlanCommand>
{
    public AssignToStudentTrainingPlanCommandValidator()
    {
        RuleFor(x => x.TrainingPlanId)
            .NotEmpty();

        RuleFor(x => x.StudentId)
            .NotEmpty();
    }
}
