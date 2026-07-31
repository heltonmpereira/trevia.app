namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record DuplicatePlanRequest(string? NewName = null, bool AssignToMe = false);
