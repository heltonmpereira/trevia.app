using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using TreviaApp.Contracts.Authentication;

namespace TreviaApp.Client.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserCachedAsync();
            if (user == null)
            {
                return new AuthenticationState(Anonymous);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName),
                new("EmailConfirmed", user.EmailConfirmed.ToString())
            };

            if (user.DisplayName != null)
                claims.Add(new Claim("DisplayName", user.DisplayName));

            foreach (var role in user.Roles ?? Array.Empty<string>())
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, "JwtAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(Anonymous);
        }
    }

    public void NotifyAuthenticationChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public static ClaimsPrincipal BuildPrincipal(CurrentUserResponse user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
        };
        foreach (var role in user.Roles ?? Array.Empty<string>())
            claims.Add(new Claim(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "JwtAuth"));
    }

    internal static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var kv = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        if (kv == null) return Enumerable.Empty<Claim>();
        var claims = new List<Claim>();
        foreach (var (k, v) in kv)
        {
            if (v is JsonElement el && el.ValueKind == JsonValueKind.Array)
            {
                claims.AddRange(el.EnumerateArray().Select(x => new Claim(k, x.ToString() ?? "")));
            }
            else
            {
                claims.Add(new Claim(k, v?.ToString() ?? ""));
            }
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }
}
