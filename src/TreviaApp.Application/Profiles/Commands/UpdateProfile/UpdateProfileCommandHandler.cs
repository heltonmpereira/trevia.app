namespace TreviaApp.Application.Profiles.Commands.UpdateProfile;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, ProfileFullResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ProfileFullResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var profile = await _db.Set<UserProfile>()
            .Include(p => p.Photo)
            .Include(p => p.Equipments)
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        profile.Update(
            request.Bio,
            request.Goal,
            request.Experience,
            request.PreferredEnvironment,
            request.PrivacyLevel,
            request.PreferredUnits);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpdateProfileHandler: SaveChangesAsync explícito concluído para ProfileId={ProfileId}", profile.Id);

        _logger.LogInformation(
            "ProfileUpdated ProfileId={ProfileId} UserId={UserId}",
            profile.Id,
            userId);

        return ProfileMappings.MapToFullResponse(profile);
    }
}
