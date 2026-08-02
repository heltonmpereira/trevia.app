using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

/// <summary>
/// Represents the ExerciseEquipment domain entity.
/// </summary>
public class ExerciseEquipment : Entity
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
    /// Gets Equipment.
    /// </summary>
    public Equipment Equipment { get; private set; }
    /// <summary>
    /// Gets Required.
    /// </summary>
    public bool Required { get; private set; } = true;

    private ExerciseEquipment() { }

    /// <summary>
    /// Initializes a new instance of the ExerciseEquipment class.
    /// </summary>
    public ExerciseEquipment(Guid exerciseId, Equipment equipment, bool required = true)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException(nameof(exerciseId));
        ExerciseId = exerciseId;
        Equipment = equipment;
        Required = required;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
