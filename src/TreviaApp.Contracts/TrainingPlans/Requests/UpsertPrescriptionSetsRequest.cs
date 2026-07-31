namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record UpsertPrescriptionSetsRequest(List<SetPrescriptionRequest> Sets);
