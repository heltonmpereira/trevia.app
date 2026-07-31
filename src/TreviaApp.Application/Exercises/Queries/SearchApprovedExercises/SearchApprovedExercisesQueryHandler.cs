namespace TreviaApp.Application.Exercises.Queries.SearchApprovedExercises;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Abstractions.Storage;
using TreviaApp.Application.Exercises.Mappings;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Enums;

public sealed class SearchApprovedExercisesQueryHandler : IQueryHandler<SearchApprovedExercisesQuery, ExerciseSearchPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly ILogger<SearchApprovedExercisesQueryHandler> _logger;

    public SearchApprovedExercisesQueryHandler(
        IApplicationDbContext db,
        IFileStorageService storage,
        ILogger<SearchApprovedExercisesQueryHandler> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    public async Task<ExerciseSearchPagedResponse> Handle(SearchApprovedExercisesQuery request, CancellationToken cancellationToken)
    {
        var r = request.Request;
        var query = _db.Set<Exercise>()
            .Include(e => e.Muscles)
            .Include(e => e.Equipments)
            .Include(e => e.Medias)
            .Where(e => e.Status == ExerciseStatus.Approved && e.Visibility == Visibility.Public);

        if (!string.IsNullOrWhiteSpace(r.Name))
            query = query.Where(e => EF.Functions.Like(e.Name, $"%{r.Name}%"));

        if (r.Environment.HasValue)
            query = query.Where(e => e.Environment == r.Environment.Value);

        if (r.Modality.HasValue)
            query = query.Where(e => e.Modality == r.Modality.Value);

        if (r.DifficultyLevel.HasValue)
            query = query.Where(e => e.DifficultyLevel == r.DifficultyLevel.Value);

        if (r.MeasurementType.HasValue)
            query = query.Where(e => e.MeasurementType == r.MeasurementType.Value);

        if (r.PrimaryMuscle.HasValue)
            query = query.Where(e => e.Muscles.Any(m =>
                m.Muscle == r.PrimaryMuscle.Value &&
                (m.MuscleRole == MuscleRole.Primary || m.MuscleRole == MuscleRole.Secondary)));

        if (r.Equipment.HasValue)
            query = query.Where(e => e.Equipments.Any(eq => eq.Equipment == r.Equipment.Value));

        query = ApplySort(query, r.SortBy, r.SortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, r.Page);
        var pageSize = Math.Clamp(r.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
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

    private static IQueryable<Exercise> ApplySort(IQueryable<Exercise> query, string? sortBy, bool desc)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "difficulty" => desc ? query.OrderByDescending(e => e.DifficultyLevel) : query.OrderBy(e => e.DifficultyLevel),
            _ => desc ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt)
        };
    }
}
