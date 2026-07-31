namespace TreviaApp.Application.Profiles.Commands.UpdateEquipments;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Shared.Enums;

public sealed record UpdateEquipmentsCommand(IEnumerable<Equipment> Equipments) : ICommand<List<Equipment>>;
