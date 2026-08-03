using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using TreviaApp.Contracts.Authentication;

namespace TreviaApp.Client.Services.Auth;

public class AuthService : IAuthService
{
    private const string AccessKey = "treviaapp_auth_access";
    private const string RefreshKey = "treviaapp_auth_refresh";
    private const string MeKey = "treviaapp_auth_me";
    private const string ExpiresKey = "treviaapp_auth_expires";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public event Func<Task>? AuthenticationChangedAsync;

    public AuthService(IHttpClientFactory factory, IJSRuntime js)
    {
        _http = factory.CreateClient("TreviaApp.AnonymousApi");
        _js = js;
    }

    private async Task NotifyChangedAsync()
    {
        if (AuthenticationChangedAsync != null)
        {
            await AuthenticationChangedAsync.Invoke();
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var access = await GetAccessTokenAsync(forceRefresh: false);
        return !string.IsNullOrEmpty(access);
    }

    public async Task<string?> GetAccessTokenAsync(bool forceRefresh = false)
    {
        try { await _lock.WaitAsync(); } catch { }
        try
        {
            var expiresStr = await _js.InvokeAsync<string?>("localStorage.getItem", ExpiresKey);
            var access = await _js.InvokeAsync<string?>("localStorage.getItem", AccessKey);
            var refresh = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey);

            if (!forceRefresh && !string.IsNullOrEmpty(access) && DateTimeOffset.TryParse(expiresStr, out var exp) && exp > DateTimeOffset.UtcNow.AddMinutes(2))
            {
                return access;
            }

            if (!string.IsNullOrEmpty(refresh))
            {
                try
                {
                    var authResp = await _http.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest
                    {
                        AccessToken = access ?? "",
                        RefreshToken = refresh
                    });
                    if (authResp.IsSuccessStatusCode)
                    {
                        var newAuth = await authResp.Content.ReadFromJsonAsync<AuthResponse>();
                        if (newAuth != null)
                        {
                            await PersistTokensAsync(newAuth);
                            return newAuth.AccessToken;
                        }
                    }
                }
                catch
                {
                    await ClearLocalStorageAsync();
                }
            }

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<CurrentUserResponse?> GetCurrentUserCachedAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", MeKey);
            if (string.IsNullOrEmpty(json))
            {
                var access = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(access)) return null;

                var resp = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "api/auth/me"));
                if (!resp.IsSuccessStatusCode) return null;
                var me = await resp.Content.ReadFromJsonAsync<CurrentUserResponse>();
                if (me != null)
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", MeKey, JsonSerializer.Serialize(me));
                }
                return me;
            }
            return JsonSerializer.Deserialize<CurrentUserResponse>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try { await _lock.WaitAsync(); } catch { }
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", request);
            await EnsureSuccessOrThrow(resp);
            var auth = (await resp.Content.ReadFromJsonAsync<AuthResponse>())!;
            await PersistTokensAsync(auth);
            await LoadAndCacheMeAsync();
            await NotifyChangedAsync();
            return auth;
        }
        finally { _lock.Release(); }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try { await _lock.WaitAsync(); } catch { }
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/register", request);
            await EnsureSuccessOrThrow(resp);
            var auth = (await resp.Content.ReadFromJsonAsync<AuthResponse>())!;
            await PersistTokensAsync(auth);
            await LoadAndCacheMeAsync();
            await NotifyChangedAsync();
            return auth;
        }
        finally { _lock.Release(); }
    }

    public async Task<AuthResponse> RefreshTokensAsync()
    {
        var access = await GetAccessTokenAsync(forceRefresh: true);
        if (string.IsNullOrEmpty(access)) throw new InvalidOperationException("Não foi possível renovar tokens.");
        var refresh = await GetRefreshTokenAsync() ?? "";
        var expires = DateTimeOffset.TryParse(await _js.InvokeAsync<string?>("localStorage.getItem", ExpiresKey), out var e) ? e : DateTimeOffset.UtcNow.AddHours(1);
        var me = await GetCurrentUserCachedAsync();
        return new AuthResponse(access, expires, refresh, me?.UserId ?? Guid.Empty, me?.Email ?? "", me?.Roles.ToList() ?? new());
    }

    public async Task LogoutAsync()
    {
        try { await _lock.WaitAsync(); } catch { }
        try
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
                var access = await _js.InvokeAsync<string?>("localStorage.getItem", AccessKey);
                if (!string.IsNullOrEmpty(access))
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
                _ = await _http.SendAsync(req);
            }
            catch { }
            await ClearLocalStorageAsync();
            await NotifyChangedAsync();
        }
        finally { _lock.Release(); }
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/change-password", request);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/forgot-password", request);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/reset-password", request);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/confirm-email", request);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task ResendConfirmationEmailAsync()
    {
        var resp = await _http.PostAsync("api/auth/resend-confirmation-email", new StringContent("", Encoding.UTF8, "application/json"));
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<UserSessionsResponse> GetActiveSessionsAsync()
    {
        var resp = await _http.GetAsync("api/auth/sessions");
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<UserSessionsResponse>())!;
    }

    public async Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/revoke", request);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task RevokeAllSessionsAsync()
    {
        var resp = await _http.DeleteAsync("api/auth/sessions");
        await EnsureSuccessOrThrow(resp);
    }

    private async Task PersistTokensAsync(AuthResponse auth)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", AccessKey, auth.AccessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshKey, auth.RefreshToken);
        await _js.InvokeVoidAsync("localStorage.setItem", ExpiresKey, auth.ExpiresAt.ToString("O"));
    }

    private async Task LoadAndCacheMeAsync()
    {
        try
        {
            var resp = await _http.GetAsync("api/auth/me");
            if (resp.IsSuccessStatusCode)
            {
                var me = await resp.Content.ReadFromJsonAsync<CurrentUserResponse>();
                if (me != null)
                    await _js.InvokeVoidAsync("localStorage.setItem", MeKey, JsonSerializer.Serialize(me));
            }
        }
        catch { }
    }

    private async Task ClearLocalStorageAsync()
    {
        foreach (var k in new[] { AccessKey, RefreshKey, ExpiresKey, MeKey })
        {
            try { await _js.InvokeVoidAsync("localStorage.removeItem", k); } catch { }
        }
    }

    private static async Task EnsureSuccessOrThrow(HttpResponseMessage resp)
    {
        if (!resp.IsSuccessStatusCode)
        {
            var content = "";
            try { content = await resp.Content.ReadAsStringAsync(); } catch { }
            throw new ApplicationException($"Falha na chamada API ({(int)resp.StatusCode}): {resp.ReasonPhrase}\n{content}");
        }
    }
}
