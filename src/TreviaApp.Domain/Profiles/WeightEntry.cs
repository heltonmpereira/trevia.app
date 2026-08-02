using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.Profiles;

/// <summary>
/// Represents the WeightEntry domain entity.
/// </summary>
public class WeightEntry : Entity
{
    /// <summary>
    /// Gets Profile Id.
    /// </summary>
    public Guid ProfileId { get; private set; }
    /// <summary>
    /// Gets Profile.
    /// </summary>
    public UserProfile Profile { get; private set; } = null!;
    /// <summary>
    /// Gets Weight Kg.
    /// </summary>
    public decimal WeightKg { get; private set; }
    /// <summary>
    /// Gets Measured At.
    /// </summary>
    public DateTimeOffset MeasuredAt { get; private set; }
    /// <summary>
    /// Gets Note.
    /// </summary>
    public string? Note { get; private set; }

    private WeightEntry() { }

    /// <summary>
    /// Initializes a new instance of the WeightEntry class.
    /// </summary>
    public WeightEntry(Guid profileId, decimal weightKg, DateTimeOffset measuredAt, string? note = null)
    {
        ProfileId = profileId;
        WeightKg = Math.Round(weightKg, 2);
        MeasuredAt = measuredAt;
        Note = note;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
