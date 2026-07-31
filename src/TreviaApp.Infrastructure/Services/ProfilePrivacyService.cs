namespace TreviaApp.Infrastructure.Services;

using TreviaApp.Application.Abstractions.Privacy;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public class ProfilePrivacyService : IProfilePrivacyService
{
    public ProfileFullResponse ApplyPrivacy(ProfileFullResponse full, UserProfile source,
        Guid? viewerUserId, IList<string> viewerRoles)
    {
        var isOwner = viewerUserId.HasValue && viewerUserId.Value == source.UserId;
        var isAdmin = viewerRoles.Contains(AppRoles.Administrator);
        var isGymManager = viewerRoles.Contains(AppRoles.GymManager);

        if (source.PrivacyLevel == PrivacyLevel.Public || isOwner || isAdmin || isGymManager)
            return full;

        if (source.PrivacyLevel == PrivacyLevel.Private)
            throw new UnauthorizedAccessException("Profile is Private");

        return full with
        {
            TotalWeighIns = 0,
            TotalMeasurements = 0,
            LatestWeightKg = null,
            LatestWeightAt = null,
            LatestHeightCm = null,
            LatestBodyFatPercent = null
        };
    }

    public bool HasAnyAccess(UserProfile profile, Guid? viewerUserId, IList<string> viewerRoles)
    {
        if (profile.PrivacyLevel == PrivacyLevel.Public) return true;
        var isOwner = viewerUserId.HasValue && viewerUserId.Value == profile.UserId;
        var isAdmin = viewerRoles.Contains(AppRoles.Administrator);
        var isGymManager = viewerRoles.Contains(AppRoles.GymManager);
        return isOwner || isAdmin || isGymManager || profile.PrivacyLevel == PrivacyLevel.FriendsOnly;
    }
}
