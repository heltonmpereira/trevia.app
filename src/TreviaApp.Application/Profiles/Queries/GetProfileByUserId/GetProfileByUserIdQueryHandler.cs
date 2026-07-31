namespace TreviaApp.Application.Profiles.Queries.GetProfileByUserId;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public sealed class GetProfileByUserIdQueryHandler : IQueryHandler<GetProfileByUserIdQuery, ProfileFullResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetProfileByUserIdQueryHandler> _logger;

    public GetProfileByUserIdQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetProfileByUserIdQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ProfileFullResponse> Handle(GetProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        var viewerId = _currentUser.UserId;

        var profile = await _db.Set<UserProfile>()
            .AsNoTracking()
            .Include(p => p.Photo)
            .Include(p => p.Equipments)
            .SingleOrDefaultAsync(p => p.UserId == request.TargetUserId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        var isOwner = viewerId == profile.UserId;
        var isAdmin = _currentUser.IsInRole(AppRoles.Administrator);
        var isGymManager = _currentUser.IsInRole(AppRoles.GymManager);

        var canViewFull = profile.PrivacyLevel == PrivacyLevel.Public
                          || isOwner
                          || isAdmin
                          || isGymManager;

        if (profile.PrivacyLevel == PrivacyLevel.Private && !canViewFull)
            throw new DomainException(
                "Este perfil é privado.",
                "ProfilePrivate");

        if (!canViewFull && profile.PrivacyLevel == PrivacyLevel.FriendsOnly)
        {
            _logger.LogInformation(
                "ProfileViewed ViewerId={ViewerId} ProfileId={ProfileId} Privacy={PrivacyLevel} Mode=Summary",
                viewerId,
                profile.Id,
                profile.PrivacyLevel);

            return ProfileMappings.MapToSummaryResponse(profile);
        }

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
            "ProfileViewed ViewerId={ViewerId} ProfileId={ProfileId} Privacy={PrivacyLevel} Mode=Full",
            viewerId,
            profile.Id,
            profile.PrivacyLevel);

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
