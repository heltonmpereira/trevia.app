namespace TreviaApp.Application.Exercises.Commands.CreateExercise;

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

public sealed class CreateExerciseCommandHandler : ICommandHandler<CreateExerciseCommand, ExerciseDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateExerciseCommandHandler> _logger;

    public CreateExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CreateExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ExerciseDetailResponse> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var slug = await SlugGenerator.GenerateUniqueSlug(
            request.Name,
            async s => await _db.Set<Exercise>()
                .AsNoTracking()
                .AnyAsync(e => e.CreatedByUserId == userId && e.Slug == s, cancellationToken),
            250);

        var exercise = new Exercise(
            userId,
            request.Name,
            slug,
            request.Environment,
            request.Modality,
            request.DifficultyLevel,
            request.MeasurementType,
            request.Instructions,
            request.ShortDescription,
            request.Tips,
            request.Tags,
            request.Visibility);

        if (request.Muscles != null)
        {
            foreach (var m in request.Muscles)
                exercise.AddMuscle(m.Muscle, m.Role, m.ActivationPercent);
        }

        if (request.Equipments != null)
        {
            foreach (var eq in request.Equipments)
                exercise.AddEquipment(eq.Equipment, eq.Required);
        }

        _db.Set<Exercise>().Add(exercise);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("CreateExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation(
            "ExerciseCreated ExerciseId={ExerciseId} UserId={UserId} Name={Name}",
            exercise.Id,
            userId,
            exercise.Name);

        return ExerciseMappings.MapToDetail(exercise);
    }
}
