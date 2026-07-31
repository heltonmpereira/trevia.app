namespace TreviaApp.Application.Consents.Queries.GetMyConsents;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Consents.Responses;

public record GetMyConsentsQuery(bool? IncludeRevoked = true) : IQuery<List<ConsentResponse>>;
