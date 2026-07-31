using TreviaApp.Domain.Identity;
using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Profiles;

public class UserProfile : AggregateRoot
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public string? Bio { get; private set; }
    public TrainingGoal Goal { get; private set; }
    public ExperienceLevel Experience { get; private set; }
    public TrainingEnvironment PreferredEnvironment { get; private set; }
    public PrivacyLevel PrivacyLevel { get; private set; } = PrivacyLevel.FriendsOnly;
    public string PreferredUnits { get; private set; } = "Metric";

    private readonly List<WeightEntry> _weightEntries = new();
    public IReadOnlyCollection<WeightEntry> WeightEntries => _weightEntries.AsReadOnly();

    private readonly List<PhysicalMeasurement> _measurements = new();
    public IReadOnlyCollection<PhysicalMeasurement> Measurements => _measurements.AsReadOnly();

    public ProfilePhoto? Photo { get; private set; }

    private readonly List<UserEquipment> _equipments = new();
    public IReadOnlyCollection<UserEquipment> Equipments => _equipments.AsReadOnly();

    private UserProfile() { }

    public UserProfile(
        Guid userId,
        TrainingGoal goal,
        ExperienceLevel experience,
        TrainingEnvironment preferredEnvironment,
        PrivacyLevel privacyLevel = PrivacyLevel.FriendsOnly,
        string preferredUnits = "Metric",
        string? bio = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        UserId = userId;
        Goal = goal;
        Experience = experience;
        PreferredEnvironment = preferredEnvironment;
        PrivacyLevel = privacyLevel;
        PreferredUnits = string.IsNullOrWhiteSpace(preferredUnits) ? "Metric" : preferredUnits;
        Bio = bio;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string? bio, TrainingGoal goal, ExperienceLevel experience,
                       TrainingEnvironment preferredEnvironment, PrivacyLevel privacyLevel,
                       string preferredUnits)
    {
        Bio = bio;
        Goal = goal;
        Experience = experience;
        PreferredEnvironment = preferredEnvironment;
        PrivacyLevel = privacyLevel;
        PreferredUnits = string.IsNullOrWhiteSpace(preferredUnits) ? "Metric" : preferredUnits;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddWeightEntry(decimal weightKg, DateTimeOffset measuredAt, string? note = null)
    {
        if (weightKg <= 0 || weightKg > 700)
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Peso deve estar em kg, entre 0 e 700.");
        if (measuredAt > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ArgumentException("Data de pesagem não pode estar no futuro.", nameof(measuredAt));

        _weightEntries.Add(new WeightEntry(this.Id, weightKg, measuredAt, note));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveWeightEntry(Guid weightEntryId)
    {
        var entry = _weightEntries.FirstOrDefault(w => w.Id == weightEntryId);
        if (entry is null) return;
        _weightEntries.Remove(entry);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddMeasurement(PhysicalMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);
        if (measurement.MeasuredAt > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new ArgumentException("Data da medida não pode estar no futuro.", nameof(measurement));

        _measurements.Add(measurement);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveMeasurement(Guid measurementId)
    {
        var m = _measurements.FirstOrDefault(x => x.Id == measurementId);
        if (m is null) return;
        _measurements.Remove(m);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPhoto(string fileId, string fileName, string contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileId)) throw new ArgumentException("FileId cannot be empty.", nameof(fileId));
        Photo = new ProfilePhoto(this.Id, fileId, fileName, contentType, sizeBytes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemovePhoto()
    {
        Photo = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateEquipments(IEnumerable<Equipment> equipments)
    {
        _equipments.Clear();
        foreach (var eq in equipments.Distinct())
        {
            _equipments.Add(new UserEquipment(this.Id, eq));
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
