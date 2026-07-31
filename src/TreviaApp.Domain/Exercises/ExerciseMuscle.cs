using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

public class ExerciseMuscle : Entity
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;
    public Muscle Muscle { get; private set; }
    public MuscleRole MuscleRole { get; private set; }
    public decimal? ActivationPercent { get; private set; }

    private ExerciseMuscle() { }

    public ExerciseMuscle(Guid exerciseId, Muscle muscle, MuscleRole role, decimal? activationPercent = null)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException(nameof(exerciseId));
        if (activationPercent.HasValue && (activationPercent < 0 || activationPercent > 100))
            throw new ArgumentOutOfRangeException(nameof(activationPercent), "Must be between 0 and 100.");
        ExerciseId = exerciseId;
        Muscle = muscle;
        MuscleRole = role;
        ActivationPercent = activationPercent.HasValue ? Math.Round(activationPercent.Value, 2) : null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
