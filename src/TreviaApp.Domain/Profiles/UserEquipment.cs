using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Profiles;

public class UserEquipment : Entity
{
    public Guid ProfileId { get; private set; }
    public UserProfile Profile { get; private set; } = null!;
    public Equipment Equipment { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private UserEquipment() { }

    public UserEquipment(Guid profileId, Equipment equipment)
    {
        ProfileId = profileId;
        Equipment = equipment;
        AddedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
