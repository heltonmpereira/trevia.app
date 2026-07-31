namespace TreviaApp.Application.Profiles.Queries.GetMeasurementHistory;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class GetMeasurementHistoryQueryHandler : IQueryHandler<GetMeasurementHistoryQuery, MeasurementHistoryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetMeasurementHistoryQueryHandler> _logger;

    public GetMeasurementHistoryQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetMeasurementHistoryQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<MeasurementHistoryResponse> Handle(GetMeasurementHistoryQuery request, CancellationToken cancellationToken)
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

        var query = _db.Set<PhysicalMeasurement>()
            .AsNoTracking()
            .Where(m => m.ProfileId == profile.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var entries = await query
            .OrderByDescending(m => m.MeasuredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => ProfileMappings.MapToMeasurementResponse(m))
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "MeasurementHistoryViewed ProfileId={ProfileId} Page={Page} PageSize={PageSize} Total={Total}",
            profile.Id,
            page,
            pageSize,
            totalCount);

        return new MeasurementHistoryResponse(
            totalCount,
            page,
            pageSize,
            entries);
    }
}
