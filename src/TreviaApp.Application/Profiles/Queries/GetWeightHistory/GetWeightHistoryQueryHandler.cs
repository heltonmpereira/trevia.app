namespace TreviaApp.Application.Profiles.Queries.GetWeightHistory;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class GetWeightHistoryQueryHandler : IQueryHandler<GetWeightHistoryQuery, WeightHistoryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetWeightHistoryQueryHandler> _logger;

    public GetWeightHistoryQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetWeightHistoryQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<WeightHistoryResponse> Handle(GetWeightHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var profile = await _db.Set<UserProfile>()
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        var query = _db.Set<WeightEntry>()
            .AsNoTracking()
            .Where(w => w.ProfileId == profile.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var allEntriesOrdered = await query
            .OrderBy(w => w.MeasuredAt)
            .ToListAsync(cancellationToken);

        var startingWeight = allEntriesOrdered.FirstOrDefault()?.WeightKg;
        var latestWeight = allEntriesOrdered.LastOrDefault()?.WeightKg;
        var changeKg = latestWeight - startingWeight;

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var entries = await query
            .OrderByDescending(w => w.MeasuredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => ProfileMappings.MapToWeightEntryResponse(w))
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "WeightHistoryViewed ProfileId={ProfileId} Page={Page} PageSize={PageSize} Total={Total}",
            profile.Id,
            page,
            pageSize,
            totalCount);

        return new WeightHistoryResponse(
            totalCount,
            page,
            pageSize,
            startingWeight,
            latestWeight,
            changeKg,
            entries);
    }
}
