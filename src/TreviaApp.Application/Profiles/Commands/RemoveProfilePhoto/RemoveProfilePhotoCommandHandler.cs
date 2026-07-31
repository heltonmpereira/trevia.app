namespace TreviaApp.Application.Profiles.Commands.RemoveProfilePhoto;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Abstractions.Storage;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class RemoveProfilePhotoCommandHandler : ICommandHandler<RemoveProfilePhotoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly ILogger<RemoveProfilePhotoCommandHandler> _logger;

    public RemoveProfilePhotoCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage,
        ILogger<RemoveProfilePhotoCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _logger = logger;
    }

    public async Task Handle(RemoveProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var profile = await _db.Set<UserProfile>()
            .Include(p => p.Photo)
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        if (profile.Photo is not null)
        {
            try
            {
                await _storage.DeleteAsync(profile.Photo.FileId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete photo FileId={FileId}", profile.Photo.FileId);
            }

            profile.RemovePhoto();

            _logger.LogInformation(
                "ProfilePhotoRemoved ProfileId={ProfileId}",
                profile.Id);
        }
    }
}
