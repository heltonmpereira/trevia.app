using Microsoft.AspNetCore.Components.Authorization;
using TreviaApp.Client.Services.Auth;

namespace TreviaApp.Client.Services;

public class AuthCurrentUserIdProvider : ICurrentUserIdProvider
{
    private readonly IAuthService _auth;

    public AuthCurrentUserIdProvider(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var me = await _auth.GetCurrentUserCachedAsync();
        return me?.UserId == Guid.Empty ? null : me?.UserId.ToString();
    }
}
