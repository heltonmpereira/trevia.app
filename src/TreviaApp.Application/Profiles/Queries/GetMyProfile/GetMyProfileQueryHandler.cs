namespace TreviaApp.Application.Profiles.Queries.GetMyProfile;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class GetMyProfileQueryHandler : IQueryHandler<GetMyProfileQuery, ProfileFullResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetMyProfileQueryHandler> _logger;

    public GetMyProfileQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetMyProfileQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ProfileFullResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var profile = await _db.Set<UserProfile>()
            .AsNoTracking()
            .Include(p => p.Photo)
            .Include(p => p.Equipments)
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        var totalWeighIns = await _db.Set<WeightEntry>()
            .AsNoTracking()
            .CountAsync(w => w.ProfileId == profile.Id, cancellationToken);

        var totalMeasurements = await _db.Set<PhysicalMeasurement>()
            .AsNoTracking()
            .CountAsync(m => m.ProfileId == profile.Id, cancellationToken);

        var latestWeight = await _db.Set<WeightEntry>()
            .AsNoTracking()
            .Where(w => w.ProfileId == profile.Id)
            .OrderByDescending(w => w.MeasuredAt)
            .Select(w => new { w.WeightKg, w.MeasuredAt })
            .FirstOrDefaultAsync(cancellationToken);

        var latestMeasurement = await _db.Set<PhysicalMeasurement>()
            .AsNoTracking()
            .Where(m => m.ProfileId == profile.Id)
            .OrderByDescending(m => m.MeasuredAt)
            .Select(m => new { m.HeightCm, m.BodyFatPercent })
            .FirstOrDefaultAsync(cancellationToken);

        _logger.LogInformation(
            "MyProfileViewed ProfileId={ProfileId} UserId={UserId}",
            profile.Id,
            userId);

        return ProfileMappings.MapToFullResponse(
            profile,
            totalWeighIns: totalWeighIns,
            totalMeasurements: totalMeasurements,
            latestWeightKg: latestWeight?.WeightKg,
            latestWeightAt: latestWeight?.MeasuredAt,
            latestHeightCm: latestMeasurement?.HeightCm,
            latestBodyFatPercent: latestMeasurement?.BodyFatPercent);
    }
}
