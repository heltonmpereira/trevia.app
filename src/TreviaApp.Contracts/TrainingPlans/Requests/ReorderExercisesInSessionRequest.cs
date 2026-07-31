namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record ReorderExercisesInSessionRequest(List<(Guid SessionExerciseId, int NewOrder)> Orders);
