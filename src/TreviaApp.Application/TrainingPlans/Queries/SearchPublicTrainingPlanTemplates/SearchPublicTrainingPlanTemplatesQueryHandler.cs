namespace TreviaApp.Application.TrainingPlans.Queries.SearchPublicTrainingPlanTemplates;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.TrainingPlans;

public sealed class SearchPublicTrainingPlanTemplatesQueryHandler : IQueryHandler<SearchPublicTrainingPlanTemplatesQuery, TrainingPlansSearchPagedResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<SearchPublicTrainingPlanTemplatesQueryHandler> _logger;

    public SearchPublicTrainingPlanTemplatesQueryHandler(
        IApplicationDbContext db,
        ILogger<SearchPublicTrainingPlanTemplatesQueryHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<TrainingPlansSearchPagedResponse> Handle(SearchPublicTrainingPlanTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .Where(tp => tp.IsPublicTemplate == true);

        if (request.SplitType.HasValue)
        {
            query = query.Where(tp => tp.SplitType == request.SplitType.Value);
        }

        if (request.MinSessions.HasValue)
        {
            query = query.Where(tp => tp.Sessions.Count >= request.MinSessions.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var search = $"%{request.SearchName}%";
            query = query.Where(tp => EF.Functions.Like(tp.Name, search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        IOrderedQueryable<TrainingPlan> ordered;
        switch ((request.SortBy ?? "popularity").ToLowerInvariant())
        {
            case "nameasc":
                ordered = query.OrderBy(tp => tp.Name);
                break;
            case "namedesc":
                ordered = query.OrderByDescending(tp => tp.Name);
                break;
            case "sessionsdesc":
                ordered = query.OrderByDescending(tp => tp.Sessions.Count);
                break;
            case "oldest":
                ordered = query.OrderBy(tp => tp.CreatedAt);
                break;
            case "popularity":
            case "newest":
            case "createdatdesc":
            default:
                ordered = query.OrderByDescending(tp => tp.CreatedAt);
                break;
        }

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var summaryItems = new List<TrainingPlanSummaryResponse>();
        foreach (var tp in items)
        {
            int totalSessions = tp.Sessions.Count;
            int totalExercises = tp.Sessions.Sum(s => s.Exercises.Count);
            summaryItems.Add(TrainingPlanMappings.MapToSummary(tp, totalSessions, totalExercises, null, null));
        }

        return new TrainingPlansSearchPagedResponse(totalCount, page, pageSize, totalPages, summaryItems);
    }
}
