namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record AssignToStudentRequest(Guid StudentId, string? NotesWhenAssigning = null);
