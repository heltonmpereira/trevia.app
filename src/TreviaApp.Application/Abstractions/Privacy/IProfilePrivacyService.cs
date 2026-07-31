namespace TreviaApp.Application.Abstractions.Privacy;

using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.Profiles;
using TreviaApp.Shared.Enums;

public interface IProfilePrivacyService
{
    ProfileFullResponse ApplyPrivacy(ProfileFullResponse full, UserProfile source,
        Guid? viewerUserId, IList<string> viewerRoles);

    bool HasAnyAccess(UserProfile profile, Guid? viewerUserId, IList<string> viewerRoles);
}
