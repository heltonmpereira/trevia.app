namespace TreviaApp.Application.Profiles.Commands.DeleteMeasurement;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class DeleteMeasurementCommandHandler : ICommandHandler<DeleteMeasurementCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteMeasurementCommandHandler> _logger;

    public DeleteMeasurementCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<DeleteMeasurementCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(DeleteMeasurementCommand request, CancellationToken cancellationToken)
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

        profile.RemoveMeasurement(request.MeasurementId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("DeleteMeasurementHandler: SaveChangesAsync explícito concluído para ProfileId={ProfileId} MeasurementId={MeasurementId}", profile.Id, request.MeasurementId);

        _logger.LogInformation(
            "MeasurementRemoved ProfileId={ProfileId} MeasurementId={MeasurementId}",
            profile.Id,
            request.MeasurementId);
    }
}
