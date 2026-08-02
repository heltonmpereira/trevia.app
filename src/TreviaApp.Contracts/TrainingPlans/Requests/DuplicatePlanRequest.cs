namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for DuplicatePlanRequest.
/// </summary>
/// <param name="NewName">New Name value.</param>
/// <param name="AssignToMe">Assign To Me value.</param>
public sealed record DuplicatePlanRequest(string? NewName = null, bool AssignToMe = false);
