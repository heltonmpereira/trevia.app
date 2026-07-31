namespace TreviaApp.Application.Profiles.Commands.UpsertMeasurement;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class UpsertMeasurementCommandHandler : ICommandHandler<UpsertMeasurementCommand, MeasurementResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpsertMeasurementCommandHandler> _logger;

    public UpsertMeasurementCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpsertMeasurementCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<MeasurementResponse> Handle(UpsertMeasurementCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var profile = await _db.Set<UserProfile>()
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        var measurement = new PhysicalMeasurement(
            profile.Id,
            request.MeasuredAt,
            request.HeightCm,
            request.WaistCm,
            request.HipCm,
            request.ChestCm,
            request.ArmLeftCm,
            request.ArmRightCm,
            request.ThighLeftCm,
            request.ThighRightCm,
            request.CalfLeftCm,
            request.CalfRightCm,
            request.BodyFatPercent,
            request.WaterPercent,
            request.MuscleMassPercent,
            request.VisceralFatRating,
            request.BmiKgM2,
            request.Note);

        profile.AddMeasurement(measurement);

        _logger.LogInformation(
            "MeasurementAdded ProfileId={ProfileId} MeasurementId={MeasurementId}",
            profile.Id,
            measurement.Id);

        return ProfileMappings.MapToMeasurementResponse(measurement);
    }
}
