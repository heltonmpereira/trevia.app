using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

public class ExerciseEquipment : Entity
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;
    public Equipment Equipment { get; private set; }
    public bool Required { get; private set; } = true;

    private ExerciseEquipment() { }

    public ExerciseEquipment(Guid exerciseId, Equipment equipment, bool required = true)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException(nameof(exerciseId));
        ExerciseId = exerciseId;
        Equipment = equipment;
        Required = required;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
