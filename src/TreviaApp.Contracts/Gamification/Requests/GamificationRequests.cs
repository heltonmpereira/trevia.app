namespace TreviaApp.Contracts.Gamification.Requests;

public sealed record AwardWorkoutPointsRequest
{
    public AwardWorkoutPointsRequest() { }

    public Guid SessionId { get; init; }
}

public sealed record AdjustPointsRequest
{
    public AdjustPointsRequest() { }

    public int Amount { get; init; }

    public string Description { get; init; } = string.Empty;
}

public sealed record ClaimMissionRequest
{
    public ClaimMissionRequest() { }

    public Guid MissionId { get; init; }

    public string Type { get; init; } = "Daily";

    public DateTime? Date { get; init; }
}
