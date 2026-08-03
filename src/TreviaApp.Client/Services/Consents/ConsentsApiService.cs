using System.Net.Http.Json;
using System.Text;
using TreviaApp.Contracts.Consents.Requests;
using TreviaApp.Contracts.Consents.Responses;

namespace TreviaApp.Client.Services.Consents;

public class ConsentsApiService : IConsentsService
{
    private readonly HttpClient _http;

    public ConsentsApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<List<ConsentResponse>> GiveConsentBatch(GiveConsentBatchRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/consents/give", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<ConsentResponse>>(ct))!;
    }

    public async Task RevokeConsent(RevokeConsentRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/consents/revoke", request, ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<List<ConsentResponse>> GetMyConsents(bool includeRevoked = true, CancellationToken ct = default)
    {
        var qs = QueryString.Build(("includeRevoked", includeRevoked.ToString().ToLowerInvariant()));
        var resp = await _http.GetAsync("api/consents/mine" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<ConsentResponse>>(ct))!;
    }

    public async Task<List<ConsentVersionInfoResponse>> GetConsentVersions(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/consents/versions", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<ConsentVersionInfoResponse>>(ct))!;
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

    private ref struct QueryString
    {
        private StringBuilder? _sb;
        public static QueryString Build(params (string Key, string Value)[] items)
        {
            var qs = new QueryString();
            foreach (var (k, v) in items)
                qs = qs.Add((k, v));
            return qs;
        }
        public QueryString Add((string Key, string Value) item)
        {
            _sb ??= new StringBuilder();
            _sb.Append(_sb.Length == 0 ? '?' : '&');
            _sb.Append(Uri.EscapeDataString(item.Key));
            _sb.Append('=');
            _sb.Append(Uri.EscapeDataString(item.Value));
            return this;
        }
        public override string ToString() => _sb?.ToString() ?? "";
    }
}
