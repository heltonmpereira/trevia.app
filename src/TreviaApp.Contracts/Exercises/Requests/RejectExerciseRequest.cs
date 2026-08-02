namespace TreviaApp.Contracts.Exercises.Requests;

/// <summary>
/// Request payload for RejectExerciseRequest.
/// </summary>
/// <param name="Reason">Reason value.</param>
public sealed record RejectExerciseRequest(string Reason);
