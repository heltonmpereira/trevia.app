namespace TreviaApp.Application.Exercises.Commands.RemoveMuscleFromExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class RemoveMuscleFromExerciseCommandHandler : ICommandHandler<RemoveMuscleFromExerciseCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RemoveMuscleFromExerciseCommandHandler> _logger;

    public RemoveMuscleFromExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<RemoveMuscleFromExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(RemoveMuscleFromExerciseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var exercise = await _db.Set<Exercise>()
            .Include(e => e.Muscles)
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (!IsOwnerOrAdminOrGymManager(exercise, userId))
            throw new DomainException("Usuário não tem permissão.", ErrorCodes.ExerciseNotOwner);

        exercise.RemoveMuscle(request.Muscle);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("RemoveMuscleFromExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation(
            "ExerciseMuscleRemoved ExerciseId={ExerciseId} Muscle={Muscle}",
            exercise.Id,
            request.Muscle);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
