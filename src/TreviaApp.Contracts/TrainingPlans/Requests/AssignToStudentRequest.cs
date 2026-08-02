namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for AssignToStudentRequest.
/// </summary>
/// <param name="StudentId">Student Id value.</param>
/// <param name="NotesWhenAssigning">Notes When Assigning value.</param>
public sealed record AssignToStudentRequest(Guid StudentId, string? NotesWhenAssigning = null);
