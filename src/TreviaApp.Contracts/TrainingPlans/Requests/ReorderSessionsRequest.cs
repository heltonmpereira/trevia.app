namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record ReorderSessionsRequest(List<(Guid SessionId, int NewOrder)> Orders);
