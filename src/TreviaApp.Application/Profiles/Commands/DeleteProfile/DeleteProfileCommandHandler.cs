namespace TreviaApp.Application.Profiles.Commands.DeleteProfile;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class DeleteProfileCommandHandler : ICommandHandler<DeleteProfileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteProfileCommandHandler> _logger;

    public DeleteProfileCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<DeleteProfileCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
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

        profile.Delete();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("DeleteProfileHandler: SaveChangesAsync explícito concluído para ProfileId={ProfileId}", profile.Id);

        _logger.LogInformation(
            "ProfileSoftDeleted ProfileId={ProfileId} UserId={UserId}",
            profile.Id,
            userId);
    }
}
