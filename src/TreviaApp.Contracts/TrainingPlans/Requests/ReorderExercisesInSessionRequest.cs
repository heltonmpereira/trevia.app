namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for ReorderExercisesInSessionRequest.
/// </summary>
/// <param name="Orders">Orders value.</param>
public sealed record ReorderExercisesInSessionRequest(List<(Guid SessionExerciseId, int NewOrder)> Orders);
