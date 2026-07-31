namespace TreviaApp.IntegrationTests.Utilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
public static class TestAuthenticationExtensions
{
    public static async Task<AuthResponse> RegisterNewStudentAsync(this HttpClient client, string email, string password = "Student123!", string firstName = "Test", string lastName = "Student")
    {
        var resp = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = firstName + " " + lastName
        });
        resp.EnsureSuccessStatusCode();
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!;
    }
    public static async Task<AuthResponse> LoginAsync(this HttpClient client, string email, string password, bool rememberMe = false)
    {
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password, RememberMe = rememberMe });
        resp.EnsureSuccessStatusCode();
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!;
    }
    public static HttpClient WithBearer(this HttpClient client, AuthResponse auth)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
    public static HttpClient WithBearer(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
