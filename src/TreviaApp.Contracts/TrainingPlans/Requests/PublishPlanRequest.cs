namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for PublishPlanRequest.
/// </summary>
/// <param name="AsPublicTemplate">As Public Template value.</param>
public sealed record PublishPlanRequest(bool AsPublicTemplate = true);
