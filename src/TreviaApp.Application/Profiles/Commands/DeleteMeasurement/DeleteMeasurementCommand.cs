namespace TreviaApp.Application.Profiles.Commands.DeleteMeasurement;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record DeleteMeasurementCommand(Guid MeasurementId) : ICommand;
