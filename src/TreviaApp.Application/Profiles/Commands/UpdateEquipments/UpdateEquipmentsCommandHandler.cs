namespace TreviaApp.Application.Profiles.Commands.UpdateEquipments;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Enums;

public sealed class UpdateEquipmentsCommandHandler : ICommandHandler<UpdateEquipmentsCommand, List<Equipment>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateEquipmentsCommandHandler> _logger;

    public UpdateEquipmentsCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpdateEquipmentsCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<Equipment>> Handle(UpdateEquipmentsCommand request, CancellationToken cancellationToken)
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

        var distinctEquipments = request.Equipments.Distinct().ToList();

        profile.UpdateEquipments(distinctEquipments);

        _logger.LogInformation(
            "EquipmentsUpdated ProfileId={ProfileId} Count={Count}",
            profile.Id,
            distinctEquipments.Count);

        return distinctEquipments;
    }
}
