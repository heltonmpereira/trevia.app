namespace TreviaApp.Contracts.Authentication;

public class UserSessionsResponse
{
    public UserSessionsResponse() { }

    public UserSessionsResponse(List<UserSessionItem> sessions)
    {
        Sessions = sessions;
    }

    public List<UserSessionItem> Sessions { get; set; } = new();
}

public class UserSessionItem
{
    public UserSessionItem() { }

    public UserSessionItem(string SessionId, string Device, string IpAddress, DateTimeOffset StartedAt, bool IsCurrent)
    {
        this.SessionId = SessionId;
        this.Device = Device;
        this.IpAddress = IpAddress;
        this.StartedAt = StartedAt;
        this.IsCurrent = IsCurrent;
    }

    public string Device { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public bool IsCurrent { get; set; }
    public string SessionId { get; set; } = string.Empty;
}
