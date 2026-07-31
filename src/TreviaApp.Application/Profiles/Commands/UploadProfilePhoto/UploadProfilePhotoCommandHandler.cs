namespace TreviaApp.Application.Profiles.Commands.UploadProfilePhoto;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Abstractions.Storage;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class UploadProfilePhotoCommandHandler : ICommandHandler<UploadProfilePhotoCommand, PhotoUploadResultResponse>
{
    private const long MaxSizeBytes = 5L * 1024 * 1024;
    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };
    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly ILogger<UploadProfilePhotoCommandHandler> _logger;

    public UploadProfilePhotoCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage,
        ILogger<UploadProfilePhotoCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _logger = logger;
    }

    public async Task<PhotoUploadResultResponse> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        if (request.SizeBytes <= 0)
            return new PhotoUploadResultResponse(false, null, "Arquivo vazio.");

        if (request.SizeBytes > MaxSizeBytes)
            return new PhotoUploadResultResponse(false, null, "Tamanho máximo de arquivo é 5MB.");

        if (!AllowedContentTypes.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
            return new PhotoUploadResultResponse(false, null, "Tipo de arquivo não suportado.");

        var ext = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            return new PhotoUploadResultResponse(false, null, "Extensão de arquivo não suportada.");

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
                _logger.LogWarning(ex, "Failed to delete old photo FileId={FileId}", profile.Photo.FileId);
            }
            profile.RemovePhoto();
        }

        var fileId = $"profile-photos/{profile.Id}/{Guid.NewGuid()}{ext}";

        using var stream = new MemoryStream(request.FileBytes);
        await _storage.UploadAsync(
            profile.Id.ToString(),
            "profile-photos",
            stream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        profile.SetPhoto(fileId, request.FileName, request.ContentType, request.SizeBytes);

        var accessUrl = await _storage.GetTemporaryUrlAsync(fileId, TimeSpan.FromHours(1), cancellationToken);

        _logger.LogInformation(
            "ProfilePhotoUploaded ProfileId={ProfileId} FileId={FileId}",
            profile.Id,
            fileId);

        return new PhotoUploadResultResponse(
            true,
            new ProfilePhotoResponse(
                profile.Photo!.Id,
                profile.Photo.FileName,
                profile.Photo.ContentType,
                profile.Photo.SizeBytes,
                profile.Photo.UploadedAt,
                accessUrl));
    }
}
