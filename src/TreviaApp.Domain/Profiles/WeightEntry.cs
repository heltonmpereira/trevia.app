using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.Profiles;

public class WeightEntry : Entity
{
    public Guid ProfileId { get; private set; }
    public UserProfile Profile { get; private set; } = null!;
    public decimal WeightKg { get; private set; }
    public DateTimeOffset MeasuredAt { get; private set; }
    public string? Note { get; private set; }

    private WeightEntry() { }

    public WeightEntry(Guid profileId, decimal weightKg, DateTimeOffset measuredAt, string? note = null)
    {
        ProfileId = profileId;
        WeightKg = Math.Round(weightKg, 2);
        MeasuredAt = measuredAt;
        Note = note;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
