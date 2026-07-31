namespace TreviaApp.Application.Profiles.Commands.UpsertMeasurement;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record UpsertMeasurementCommand(
    DateTimeOffset MeasuredAt,
    decimal? HeightCm = null,
    decimal? WaistCm = null,
    decimal? HipCm = null,
    decimal? ChestCm = null,
    decimal? ArmLeftCm = null,
    decimal? ArmRightCm = null,
    decimal? ThighLeftCm = null,
    decimal? ThighRightCm = null,
    decimal? CalfLeftCm = null,
    decimal? CalfRightCm = null,
    decimal? BodyFatPercent = null,
    decimal? WaterPercent = null,
    decimal? MuscleMassPercent = null,
    decimal? VisceralFatRating = null,
    decimal? BmiKgM2 = null,
    string? Note = null) : ICommand<MeasurementResponse>;
