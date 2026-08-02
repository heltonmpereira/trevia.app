using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Requests;

/// <summary>
/// Request payload for UpdateEquipmentsRequest.
/// </summary>
/// <param name="Equipments">Equipments value.</param>
public sealed record UpdateEquipmentsRequest(IEnumerable<Equipment> Equipments);
