namespace TreviaApp.Application.Exercises.Commands.AddEquipmentToExercise;

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

public sealed class AddEquipmentToExerciseCommandHandler : ICommandHandler<AddEquipmentToExerciseCommand, ExerciseEquipmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AddEquipmentToExerciseCommandHandler> _logger;

    public AddEquipmentToExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<AddEquipmentToExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ExerciseEquipmentResponse> Handle(AddEquipmentToExerciseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var exercise = await _db.Set<Exercise>()
            .Include(e => e.Equipments)
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (!IsOwnerOrAdminOrGymManager(exercise, userId))
            throw new DomainException("Usuário não tem permissão.", ErrorCodes.ExerciseNotOwner);

        exercise.AddEquipment(request.Equipment, request.Required);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AddEquipmentToExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        var added = exercise.Equipments
            .OrderByDescending(eq => eq.CreatedAt)
            .First(eq => eq.Equipment == request.Equipment);

        _logger.LogInformation(
            "ExerciseEquipmentAdded ExerciseId={ExerciseId} Equipment={Equipment} Required={Required}",
            exercise.Id,
            request.Equipment,
            request.Required);

        return ExerciseMappings.MapToEquipmentResponse(added);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
