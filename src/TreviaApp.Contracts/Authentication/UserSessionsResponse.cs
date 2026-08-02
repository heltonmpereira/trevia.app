namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the UserSessionsResponse contract.
/// </summary>
public class UserSessionsResponse
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserSessionsResponse"/>.
    /// </summary>
    public UserSessionsResponse() { }

    /// <summary>
    /// Initializes a new instance of <see cref="UserSessionsResponse"/>.
    /// </summary>
    public UserSessionsResponse(List<UserSessionItem> sessions)
    {
        Sessions = sessions;
    }

    /// <summary>
    /// Gets or sets Sessions.
    /// </summary>
    public List<UserSessionItem> Sessions { get; set; } = new();
}

/// <summary>
/// Represents the UserSessionItem contract.
/// </summary>
public class UserSessionItem
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserSessionItem"/>.
    /// </summary>
    public UserSessionItem() { }

    /// <summary>
    /// Initializes a new instance of <see cref="UserSessionItem"/>.
    /// </summary>
    public UserSessionItem(string SessionId, string Device, string IpAddress, DateTimeOffset StartedAt, bool IsCurrent)
    {
        this.SessionId = SessionId;
        this.Device = Device;
        this.IpAddress = IpAddress;
        this.StartedAt = StartedAt;
        this.IsCurrent = IsCurrent;
    }

    /// <summary>
    /// Gets or sets Device.
    /// </summary>
    public string Device { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Ip Address.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Started At.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }
    /// <summary>
    /// Gets or sets Is Current.
    /// </summary>
    public bool IsCurrent { get; set; }
    /// <summary>
    /// Gets or sets Session Id.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
}
