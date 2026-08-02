namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for UpsertPrescriptionSetsRequest.
/// </summary>
/// <param name="Sets">Sets value.</param>
public sealed record UpsertPrescriptionSetsRequest(List<SetPrescriptionRequest> Sets);
