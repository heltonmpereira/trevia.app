using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Profiles;

/// <summary>
/// Represents the UserEquipment domain entity.
/// </summary>
public class UserEquipment : Entity
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
    /// Gets Equipment.
    /// </summary>
    public Equipment Equipment { get; private set; }
    /// <summary>
    /// Gets Added At.
    /// </summary>
    public DateTimeOffset AddedAt { get; private set; }

    private UserEquipment() { }

    /// <summary>
    /// Initializes a new instance of the UserEquipment class.
    /// </summary>
    public UserEquipment(Guid profileId, Equipment equipment)
    {
        ProfileId = profileId;
        Equipment = equipment;
        AddedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
