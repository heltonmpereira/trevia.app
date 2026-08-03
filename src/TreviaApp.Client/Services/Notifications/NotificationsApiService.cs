using System.Net.Http.Json;
using System.Text;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Notifications.Responses;

namespace TreviaApp.Client.Services.Notifications;

public class NotificationsApiService : INotificationsService
{
    private readonly HttpClient _http;

    public NotificationsApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<UnreadCountResponse> GetUnreadCount(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/notifications/unread-count", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<UnreadCountResponse>(ct))!;
    }

    public async Task<PaginatedResponse<NotificationResponse>> GetMyNotifications(int page = 1, int pageSize = 50, bool onlyUnread = false, CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()),
            ("onlyUnread", onlyUnread.ToString().ToLowerInvariant()));
        var resp = await _http.GetAsync("api/notifications" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PaginatedResponse<NotificationResponse>>(ct))!;
    }

    public async Task<NotificationResponse> GetNotificationById(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/notifications/" + id, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<NotificationResponse>(ct))!;
    }

    public async Task<NotificationResponse> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync("api/notifications/" + id + "/read", new StringContent("", Encoding.UTF8, "application/json"), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<NotificationResponse>(ct))!;
    }

    public async Task<MarkManyResultResponse> MarkAllAsRead(CancellationToken ct = default)
    {
        var resp = await _http.PutAsync("api/notifications/read-all", new StringContent("", Encoding.UTF8, "application/json"), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<MarkManyResultResponse>(ct))!;
    }

    public async Task DeleteNotification(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync("api/notifications/" + id, ct);
        await EnsureSuccessOrThrow(resp);
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
