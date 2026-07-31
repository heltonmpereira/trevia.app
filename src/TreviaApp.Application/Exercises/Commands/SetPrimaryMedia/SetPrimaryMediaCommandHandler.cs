namespace TreviaApp.Application.Exercises.Commands.SetPrimaryMedia;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class SetPrimaryMediaCommandHandler : ICommandHandler<SetPrimaryMediaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SetPrimaryMediaCommandHandler> _logger;

    public SetPrimaryMediaCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<SetPrimaryMediaCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(SetPrimaryMediaCommand request, CancellationToken cancellationToken)
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
            throw new DomainException("Usuário não tem permissão.", ErrorCodes.ExerciseNotOwner);

        var media = exercise.Medias.FirstOrDefault(m => m.Id == request.MediaId);
        if (media is null)
            throw new DomainException("Mídia não encontrada.", ErrorCodes.ExerciseMediaNotFound);

        exercise.SetPrimaryMedia(request.MediaId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("SetPrimaryMediaHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation(
            "ExercisePrimaryMediaSet ExerciseId={ExerciseId} MediaId={MediaId}",
            exercise.Id,
            request.MediaId);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
