namespace TreviaApp.Application.Exercises.Queries.GetExerciseById;

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
using TreviaApp.Shared.Enums;

public sealed class GetExerciseByIdQueryHandler : IQueryHandler<GetExerciseByIdQuery, ExerciseDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly ILogger<GetExerciseByIdQueryHandler> _logger;

    public GetExerciseByIdQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage,
        ILogger<GetExerciseByIdQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _logger = logger;
    }

    public async Task<ExerciseDetailResponse> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _db.Set<Exercise>()
            .Include(e => e.Muscles)
            .Include(e => e.Equipments)
            .Include(e => e.Medias)
            .Include(e => e.CreatedByUser)
            .Include(e => e.ApprovedByUser)
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        var userId = _currentUser.UserId;
        var hasAccess = (userId.HasValue && userId.Value == exercise.CreatedByUserId)
                        || _currentUser.IsInRole(AppRoles.Administrator)
                        || _currentUser.IsInRole(AppRoles.GymManager);

        if (exercise.Visibility == Visibility.Private && !hasAccess)
            throw new DomainException("Você não tem permissão para visualizar este exercício.", ErrorCodes.Forbidden);

        var mediaUrls = new Dictionary<Guid, string>();
        if (hasAccess || exercise.Visibility == Visibility.Public)
        {
            foreach (var media in exercise.Medias)
            {
                var url = await _storage.GetTemporaryUrlAsync(media.FileId, TimeSpan.FromHours(24), cancellationToken);
                mediaUrls[media.Id] = url;
            }
        }

        var createdByName = exercise.CreatedByUser?.DisplayName ?? exercise.CreatedByUser?.Email;
        var approvedByName = exercise.ApprovedByUser?.DisplayName ?? exercise.ApprovedByUser?.Email;

        return ExerciseMappings.MapToDetail(exercise, createdByName, approvedByName, mediaUrls);
    }
}
