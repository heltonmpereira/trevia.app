namespace TreviaApp.Application.Exercises.Queries.GetMyExercises;

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

public sealed class GetMyExercisesQueryHandler : IQueryHandler<GetMyExercisesQuery, ExerciseSearchPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly ILogger<GetMyExercisesQueryHandler> _logger;

    public GetMyExercisesQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage,
        ILogger<GetMyExercisesQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _logger = logger;
    }

    public async Task<ExerciseSearchPagedResponse> Handle(GetMyExercisesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var query = _db.Set<Exercise>()
            .Include(e => e.Muscles)
            .Include(e => e.Equipments)
            .Include(e => e.Medias)
            .Where(e => e.CreatedByUserId == userId);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var summaryItems = new List<ExerciseSummaryResponse>();
        foreach (var ex in items)
        {
            var primaryMedia = ex.Medias.FirstOrDefault(m => m.IsPrimary) ?? ex.Medias.OrderBy(m => m.Order).FirstOrDefault();
            string? primaryUrl = null;
            if (primaryMedia != null)
                primaryUrl = await _storage.GetTemporaryUrlAsync(primaryMedia.FileId, TimeSpan.FromHours(24), cancellationToken);

            summaryItems.Add(ExerciseMappings.MapToSummary(ex, primaryUrl));
        }

        return new ExerciseSearchPagedResponse(totalCount, page, pageSize, totalPages, summaryItems);
    }
}
