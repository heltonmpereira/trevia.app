using TreviaApp.Contracts.Consents.Requests;
using TreviaApp.Contracts.Consents.Responses;

namespace TreviaApp.Client.Services.Consents;

public interface IConsentsService
{
    Task<List<ConsentResponse>> GiveConsentBatch(GiveConsentBatchRequest request, CancellationToken ct = default);
    Task RevokeConsent(RevokeConsentRequest request, CancellationToken ct = default);
    Task<List<ConsentResponse>> GetMyConsents(bool includeRevoked = true, CancellationToken ct = default);
    Task<List<ConsentVersionInfoResponse>> GetConsentVersions(CancellationToken ct = default);
}
