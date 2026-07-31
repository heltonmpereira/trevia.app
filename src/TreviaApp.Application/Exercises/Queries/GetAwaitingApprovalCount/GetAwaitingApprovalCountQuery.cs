namespace TreviaApp.Application.Exercises.Queries.GetAwaitingApprovalCount;

using TreviaApp.Application.Abstractions.Messaging;

public sealed record GetAwaitingApprovalCountQuery : IQuery<int>;
