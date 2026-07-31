namespace TreviaApp.Application.Exercises.Commands.AddMuscleToExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Exercises.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class AddMuscleToExerciseCommandHandler : ICommandHandler<AddMuscleToExerciseCommand, ExerciseMuscleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AddMuscleToExerciseCommandHandler> _logger;

    public AddMuscleToExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<AddMuscleToExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ExerciseMuscleResponse> Handle(AddMuscleToExerciseCommand request, CancellationToken cancellationToken)
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

        exercise.AddMuscle(request.Muscle, request.Role, request.ActivationPercent);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AddMuscleToExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        var added = exercise.Muscles
            .OrderByDescending(m => m.CreatedAt)
            .First(m => m.Muscle == request.Muscle);

        _logger.LogInformation(
            "ExerciseMuscleAdded ExerciseId={ExerciseId} Muscle={Muscle} Role={Role}",
            exercise.Id,
            request.Muscle,
            request.Role);

        return ExerciseMappings.MapToMuscleResponse(added);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
