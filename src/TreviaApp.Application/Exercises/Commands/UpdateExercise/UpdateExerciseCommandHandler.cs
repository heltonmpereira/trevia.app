namespace TreviaApp.Application.Exercises.Commands.UpdateExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Common;
using TreviaApp.Application.Exercises.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class UpdateExerciseCommandHandler : ICommandHandler<UpdateExerciseCommand, ExerciseDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateExerciseCommandHandler> _logger;

    public UpdateExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpdateExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ExerciseDetailResponse> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var exercise = await _db.Set<Exercise>()
            .Include(e => e.Muscles)
            .Include(e => e.Equipments)
            .Include(e => e.Medias)
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (!IsOwnerOrAdminOrGymManager(exercise, userId))
            throw new DomainException("Usuário não tem permissão para editar este exercício.", ErrorCodes.ExerciseNotOwner);

        var slug = exercise.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)
            ? exercise.Slug
            : await SlugGenerator.GenerateUniqueSlug(
                request.Name,
                async s => await _db.Set<Exercise>()
                    .AsNoTracking()
                    .AnyAsync(e => e.CreatedByUserId == exercise.CreatedByUserId && e.Slug == s && e.Id != exercise.Id, cancellationToken),
                250);

        exercise.Update(
            request.Name,
            slug,
            request.Instructions,
            request.ShortDescription,
            request.Tips,
            request.Tags,
            request.Environment,
            request.Modality,
            request.DifficultyLevel,
            request.MeasurementType,
            request.Visibility);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpdateExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation("ExerciseUpdated ExerciseId={Id}", exercise.Id);

        return ExerciseMappings.MapToDetail(exercise);
    }

    private bool IsOwnerOrAdminOrGymManager(Exercise exercise, Guid? currentUserId)
        => (currentUserId.HasValue && currentUserId.Value == exercise.CreatedByUserId)
           || _currentUser.IsInRole(AppRoles.Administrator)
           || _currentUser.IsInRole(AppRoles.GymManager);
}
