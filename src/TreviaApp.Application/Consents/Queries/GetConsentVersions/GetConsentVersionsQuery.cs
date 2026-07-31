namespace TreviaApp.Application.Consents.Queries.GetConsentVersions;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Consents.Responses;

public record GetConsentVersionsQuery() : IQuery<List<ConsentVersionInfoResponse>>;
