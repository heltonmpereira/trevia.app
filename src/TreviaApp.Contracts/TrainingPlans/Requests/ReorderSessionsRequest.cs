namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for ReorderSessionsRequest.
/// </summary>
/// <param name="Orders">Orders value.</param>
public sealed record ReorderSessionsRequest(List<(Guid SessionId, int NewOrder)> Orders);
