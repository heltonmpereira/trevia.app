namespace TreviaApp.Application.Consents.Queries.GetConsentVersions;
using MediatR;
using TreviaApp.Contracts.Consents.Responses;
using TreviaApp.Shared.Enums;

public class GetConsentVersionsQueryHandler : IQueryHandler<GetConsentVersionsQuery, List<ConsentVersionInfoResponse>>
{
    public Task<List<ConsentVersionInfoResponse>> Handle(GetConsentVersionsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTimeOffset.UtcNow;
        var lastMonth = today.AddMonths(-1);

        var versions = new List<ConsentVersionInfoResponse>
        {
            new(ConsentType.TermsOfService, "1.0.0", lastMonth),
            new(ConsentType.PrivacyPolicy, "1.0.0", today),
            new(ConsentType.HealthDataProcessing, "1.0.0", today),
            new(ConsentType.MarketingCommunication, "1.0.0", today),
            new(ConsentType.MarketingCommunications, "1.0.0", today),
            new(ConsentType.ThirdPartySharing, "1.0.0", today),
            new(ConsentType.CookiePreferences, "1.0.0", today),
            new(ConsentType.DataProcessing, "1.0.0", today)
        };

        return Task.FromResult(versions);
    }
}
