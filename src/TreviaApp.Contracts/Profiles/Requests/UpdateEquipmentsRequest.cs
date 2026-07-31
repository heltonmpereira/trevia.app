using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Requests;

public sealed record UpdateEquipmentsRequest(IEnumerable<Equipment> Equipments);
