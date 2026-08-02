using FluentValidation;
using static TreviaApp.Application.WorkoutExecution.Commands.ExerciseActions;

namespace TreviaApp.Application.WorkoutExecution.Commands;

public sealed class SkipWorkoutExerciseCommandValidator : AbstractValidator<SkipWorkoutExerciseCommand>
{
    public SkipWorkoutExerciseCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.WorkoutSessionId).NotEmpty();
        RuleFor(x => x.WorkoutExerciseId).NotEmpty();
        RuleFor(x => x.SkipReason).MaximumLength(500).When(x => x.SkipReason != null);
    }
}

public sealed class AddExtraSetToExerciseCommandValidator : AbstractValidator<AddExtraSetToExerciseCommand>
{
    public AddExtraSetToExerciseCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.WorkoutSessionId).NotEmpty();
        RuleFor(x => x.WorkoutExerciseId).NotEmpty();
        RuleFor(x => x.SuggestedSetNumber).GreaterThanOrEqualTo(1).When(x => x.SuggestedSetNumber.HasValue);
    }
}

public sealed class LogWorkoutSetCommandValidator : AbstractValidator<LogWorkoutSetCommand>
{
    public LogWorkoutSetCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.WorkoutSessionId).NotEmpty();
        RuleFor(x => x.WorkoutExerciseId).NotEmpty();
        RuleFor(x => x.WorkoutSetId).NotEmpty();

        RuleFor(x => x.ActualReps).GreaterThanOrEqualTo(0).When(x => x.ActualReps.HasValue);
        RuleFor(x => x.ActualLoadValue).GreaterThanOrEqualTo(0).When(x => x.ActualLoadValue.HasValue);
        RuleFor(x => x.ActualDurationSeconds).GreaterThanOrEqualTo(0).When(x => x.ActualDurationSeconds.HasValue);
        RuleFor(x => x.DistanceKm).GreaterThanOrEqualTo(0).When(x => x.DistanceKm.HasValue);
        RuleFor(x => x.SpeedKmh).GreaterThanOrEqualTo(0).When(x => x.SpeedKmh.HasValue);
        RuleFor(x => x.InclinePercent).GreaterThanOrEqualTo(-100).LessThanOrEqualTo(100).When(x => x.InclinePercent.HasValue);
        RuleFor(x => x.Calories).GreaterThanOrEqualTo(0).When(x => x.Calories.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
