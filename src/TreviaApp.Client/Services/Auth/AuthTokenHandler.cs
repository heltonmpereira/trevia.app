using System.Net.Http.Headers;

namespace TreviaApp.Client.Services.Auth;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthTokenHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath.StartsWith("/api/") == true)
        {
            var token = await _authService.GetAccessTokenAsync(forceRefresh: false);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                request.Headers.Add("X-Client-Request-Id", Guid.NewGuid().ToString());
            }
            catch
            {
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
            request.RequestUri?.AbsolutePath.StartsWith("/api/") == true)
        {
            var refreshed = await _authService.GetAccessTokenAsync(forceRefresh: true);
            if (!string.IsNullOrEmpty(refreshed))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);
                return await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}
