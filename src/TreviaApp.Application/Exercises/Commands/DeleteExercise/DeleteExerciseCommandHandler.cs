namespace TreviaApp.Application.Exercises.Commands.DeleteExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class DeleteExerciseCommandHandler : ICommandHandler<DeleteExerciseCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteExerciseCommandHandler> _logger;

    public DeleteExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<DeleteExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var exercise = await _db.Set<Exercise>()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (!IsOwnerOrAdminOrGymManager(exercise, userId))
            throw new DomainException("Usuário não tem permissão para deletar este exercício.", ErrorCodes.ExerciseNotOwner);

        exercise.Delete();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("DeleteExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation("ExerciseDeleted ExerciseId={Id} UserId={UserId}", exercise.Id, userId);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
