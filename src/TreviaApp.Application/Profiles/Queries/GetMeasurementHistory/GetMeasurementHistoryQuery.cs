namespace TreviaApp.Application.Profiles.Queries.GetMeasurementHistory;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;

public sealed record GetMeasurementHistoryQuery(int Page = 1, int PageSize = 20) : IQuery<MeasurementHistoryResponse>;
