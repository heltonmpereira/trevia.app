namespace TreviaApp.Application.Consents.Commands.GiveConsentBatch;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Consents.Requests;
using TreviaApp.Contracts.Consents.Responses;

public record GiveConsentBatchCommand(IEnumerable<GiveConsentRequest> Consents)
    : ICommand<List<ConsentResponse>>;
