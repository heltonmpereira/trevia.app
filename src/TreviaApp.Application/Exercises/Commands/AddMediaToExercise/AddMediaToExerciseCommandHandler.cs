namespace TreviaApp.Application.Exercises.Commands.AddMediaToExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Abstractions.Storage;
using TreviaApp.Application.Exercises.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class AddMediaToExerciseCommandHandler : ICommandHandler<AddMediaToExerciseCommand, ExerciseMediaResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly ILogger<AddMediaToExerciseCommandHandler> _logger;

    public AddMediaToExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage,
        ILogger<AddMediaToExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _logger = logger;
    }

    public async Task<ExerciseMediaResponse> Handle(AddMediaToExerciseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var exercise = await _db.Set<Exercise>()
            .Include(e => e.Medias)
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (!IsOwnerOrAdminOrGymManager(exercise, userId))
            throw new DomainException("Usuário não tem permissão para adicionar mídia.", ErrorCodes.ExerciseNotOwner);

        var fileId = $"exercises/{exercise.Id}/{Guid.NewGuid():N}{System.IO.Path.GetExtension(request.FileName)}";

        using var stream = new MemoryStream(request.FileBytes);
        await _storage.UploadAsync(
            exercise.Id.ToString(),
            "medias",
            stream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var mediaId = exercise.AddMedia(
            fileId,
            request.FileName,
            request.MediaType,
            request.Order,
            request.Caption,
            request.IsPrimary,
            request.SizeBytes);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AddMediaToExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        var media = exercise.Medias.First(m => m.Id == mediaId);

        var accessUrl = await _storage.GetTemporaryUrlAsync(fileId, TimeSpan.FromHours(24), cancellationToken);

        _logger.LogInformation(
            "ExerciseMediaAdded ExerciseId={ExerciseId} MediaId={MediaId} FileId={FileId}",
            exercise.Id,
            mediaId,
            fileId);

        return ExerciseMappings.MapToMediaResponse(media, accessUrl);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
