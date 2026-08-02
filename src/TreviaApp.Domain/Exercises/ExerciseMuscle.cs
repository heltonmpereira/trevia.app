using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

/// <summary>
/// Represents the ExerciseMuscle domain entity.
/// </summary>
public class ExerciseMuscle : Entity
{
    /// <summary>
    /// Gets Exercise Id.
    /// </summary>
    public Guid ExerciseId { get; private set; }
    /// <summary>
    /// Gets Exercise.
    /// </summary>
    public Exercise Exercise { get; private set; } = null!;
    /// <summary>
    /// Gets Muscle.
    /// </summary>
    public Muscle Muscle { get; private set; }
    /// <summary>
    /// Gets Muscle Role.
    /// </summary>
    public MuscleRole MuscleRole { get; private set; }
    /// <summary>
    /// Gets Activation Percent.
    /// </summary>
    public decimal? ActivationPercent { get; private set; }

    private ExerciseMuscle() { }

    /// <summary>
    /// Initializes a new instance of the ExerciseMuscle class.
    /// </summary>
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
