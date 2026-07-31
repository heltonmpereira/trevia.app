namespace TreviaApp.Application.Profiles.Queries.GetWeightHistory;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record GetWeightHistoryQuery(int Page = 1, int PageSize = 30) : IQuery<WeightHistoryResponse>;
